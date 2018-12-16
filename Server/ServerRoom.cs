using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Shared;
using CommandType = Shared.CommandType;

namespace Server {
    /// <summary>
    /// The UDP chat's server entry point.
    /// </summary>
    public class ServerRoom {
        /// <summary>
        /// The signature of a method capable of handling a dispatched event
        /// coming from a given endpoint.
        /// </summary>
        /// <param name="receivedMessage">The message that we received and that needs to be handled.</param>
        /// <param name="clientEndPoint">The remote endpoint from whom the message came from.</param>
        private delegate void CommandHandler(ChatMessage receivedMessage, EndPoint clientEndPoint);

        private static readonly List<ServerRoom> SERVER_ROOMS = new List<ServerRoom>();

        /// <summary>
        /// The server socket from which we will bind ourselves and listen for incoming messages.
        /// </summary>
        private Socket _serverSocket;

        /// <summary>
        /// The endpoint the socket will or is binded to.
        /// </summary>
        private IPEndPoint _bindedEndpoint;

        /// <summary>
        /// Public setter for the server's <see cref="Socket"/> to listen
        /// and manipulate. This is useful to mock the socket in tests.
        /// </summary>
        public Socket ServerSocket {
            set => this._serverSocket = value;
        }

        public IPEndPoint GetListeningEndpoint() {
            return this._bindedEndpoint;
        }

        /// <summary>
        /// The list of commands supported by this server and their handler.
        ///
        /// <code>
        ///     COMMAND => ServerProgram.handle_COMMAND
        /// </code>
        /// </summary>
        private readonly Dictionary<Command, CommandHandler> COMMAND_DISPATCHERS;

        /// <summary>
        /// The list of stored messages, which gets populated by <see cref="Command.GET"/>,
        /// in other words, <see cref="handle_POST"/>.
        /// </summary>
        public readonly List<ChatMessage> STORED_CHAT_MESSAGES = new List<ChatMessage>();

        /// <summary>
        /// The list of subscribed <see cref="EndPoint"/>s to receive newly posted
        /// messages. They get subscribed through <see cref="handle_SUB"/>, thus,
        /// this list gets populated by it.
        ///
        /// This gets used by <see cref="handle_POST"/> to relay posted messages
        /// to subscribed endpoints.
        ///
        /// Then gets used by <see cref="handle_UNSUB"/> to stop relaying posted messages
        /// to a given endpoint.
        /// </summary>
        public readonly HashSet<EndPoint> SUBSCRIBERS = new HashSet<EndPoint>();

        /// <summary>
        /// Logs a given formatted message into the server's stdout.
        /// </summary>
        /// <param name="format">The <see cref="String"/> message to format.</param>
        /// <param name="arg">The formatting arguments.</param>
        private static void LogInfo(string format, params object[] arg) {
            Console.WriteLine("[Server INFO] " + "[" + DateTime.Now + "] " + format, arg: arg);
        }

        public ServerRoom() {
            this.COMMAND_DISPATCHERS = new Dictionary<Command, CommandHandler> {
                {Command.GET, this.handle_GET},
                {Command.POST, this.handle_POST},
                {Command.SUB, this.handle_SUB},
                {Command.UNSUB, this.handle_UNSUB},
                {Command.STOP, this.handle_STOP},
                {Command.CREATEROOM, this.handle_CREATEROOM},
                {Command.LISTROOMS, this.handle_LISTROOMS}
            };
        }

        /// <summary>
        /// Sends a given <see cref="ChatMessage"/> to a given <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="chatMessage">The message to send.</param>
        /// <param name="remoteEndPoint">The endpoint to send to.</param>
        public void SendMessage(ChatMessage chatMessage, EndPoint remoteEndPoint) {
            // Send the message
            var sentBytes = IPUtils.SendMessage(
                this._serverSocket, chatMessage, remoteEndPoint);

            // Log the fact we just sent data out
            LogInfo("Sent {0} bytes to {1}", sentBytes, remoteEndPoint);
        }

        /// <summary>
        /// Handles a <see cref="Command.GET"/> request,
        /// sending every <see cref="STORED_CHAT_MESSAGES">stored chat messages</see>
        /// to the requesting client.
        /// </summary>
        /// <param name="receivedMessage">The message that requested</param>
        /// <param name="clientEndPoint">The endpoint that requested our stored data.</param>
        public void handle_GET(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Send every stored message to the client
            foreach (var storedChatMessage in this.STORED_CHAT_MESSAGES) {
                this.SendMessage(storedChatMessage, clientEndPoint);
            }
        }

        /// <summary>
        /// Handle the a <see cref="Command.POST"/> request,
        /// storing the requester's message to the server,
        /// and sending it out to every subscriber.
        /// </summary>
        /// <param name="receivedMessage">The message received.</param>
        /// <param name="clientEndPoint">The remote sender's endpoint.</param>
        public void handle_POST(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Change the command type to response,
            // as we will only use it from `handle_GET` as response a message.
            receivedMessage.CommandType = CommandType.RESPONSE;

            // Store the message
            this.STORED_CHAT_MESSAGES.Add(receivedMessage);

            // Log what we just did
            LogInfo(
                "{0} just stored a {1} characters-long message",
                clientEndPoint, receivedMessage.Data.Length);

            // Send the message to every subscriber
            foreach (var subscriberEndPoint in this.SUBSCRIBERS) {
                this.SendMessage(receivedMessage, subscriberEndPoint);
            }
        }

        /// <summary>
        /// Handle the a <see cref="Command.SUB"/> request,
        /// add the requester's endpoint to the subscribers list.
        /// </summary>
        /// <param name="receivedMessage">The message received.</param>
        /// <param name="clientEndPoint">The remote sender's endpoint.</param>
        public void handle_SUB(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Add the user to subscribers list if not already subbed
            if (this.SUBSCRIBERS.Add(clientEndPoint)) {
                // Log the new subscriber
                LogInfo(
                    "{0} just subscribed!", clientEndPoint);
            }
        }

        /// <summary>
        /// Handle the a <see cref="Command.UNSUB"/> request,
        /// removing the user from the subscribers list if existing.
        /// </summary>
        /// <param name="receivedMessage">The message received.</param>
        /// <param name="clientEndPoint">The remote sender's endpoint.</param>
        public void handle_UNSUB(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Attempt to remove the user from the subscribers list
            if (this.SUBSCRIBERS.Remove(clientEndPoint)) {
                // Log the new subscriber
                LogInfo(
                    "{0} just unsubscribed!", clientEndPoint);
            }
        }

        /// <summary>
        /// Handle the a <see cref="Command.STOP"/> request, shutdown the server.
        /// </summary>
        /// <param name="receivedMessage">The message received.</param>
        /// <param name="clientEndPoint">The remote sender's endpoint.</param>
        public void handle_STOP(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            this._serverSocket.Close();
        }

        // TODO: we will have to limit the room count to 10
        public void handle_CREATEROOM(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Create a new room object
            var newRoom = new ServerRoom();

            // Start the new room in a new thread
            new Thread(() => newRoom.Listen(0)).Start();
        }

        public void handle_LISTROOMS(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            var roomList = new StringBuilder();

            // FIXME: we show the server's binding point. Not the external IP address
            // (no matter if it is public or private). Maybe we would only want to show the port?
            SERVER_ROOMS.ForEach(
                room => roomList.AppendFormat("\n\t{0}", room.GetListeningEndpoint().Port));

            var responseMessage = new ChatMessage(
                Command.LISTROOMS, CommandType.RESPONSE, "Server", roomList.ToString());

            this.SendMessage(responseMessage, clientEndPoint);
        }

        /// <summary>
        /// Handle a received message from a given client.
        /// </summary>
        /// <param name="chatMessage">The message to handle.</param>
        /// <param name="clientEndPoint">The client endpoint that the message came from.</param>
        private void HandleMessage(ChatMessage chatMessage, EndPoint clientEndPoint) {
            // Log the message
            LogInfo("{0}: {1}", clientEndPoint, chatMessage);

            // Dispatch the received command if it's not unknown
            if (this.COMMAND_DISPATCHERS.TryGetValue(chatMessage.Command, out var foundHandler)) {
                foundHandler(receivedMessage: chatMessage, clientEndPoint: clientEndPoint);
            }
        }

        /// <summary>
        /// Listens for messages and handle them.
        /// </summary>
        private void ProcessMessages() {
            EndPoint clientEndPoint = null;
            try {
                // Wait for a message, retrieve it and decode it
                var receivedMessage = IPUtils.ReceiveMessage(this._serverSocket, out clientEndPoint);

                // Handle the received message
                this.HandleMessage(receivedMessage, clientEndPoint);
            }
            catch (SyntaxErrorException) {
                LogInfo("Received an invalid message.");
            }
            catch (SocketException exc) {
                LogInfo(
                    "A connection with {0} was not properly ended. Reason: {1}",
                    clientEndPoint, exc.Message);
            }
        }

        /// <summary>
        /// Listens forever for incoming messages on a given <see cref="listeningPort"/> through
        /// a newly created socket.
        /// </summary>
        /// <param name="listeningPort"></param>
        public void Listen(ushort listeningPort) {
            // Create the listening UDP socket
            this._serverSocket = new Socket(
                AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Create the base endpoint to listen to
            this._bindedEndpoint = new IPEndPoint(IPAddress.Any, listeningPort);

            // Register the server room as active
            SERVER_ROOMS.Add(this);

            try {
                // Bind the server socket to the given port
                this._serverSocket.Bind(this.GetListeningEndpoint());

                // Retrieve the new endpoint, if it was changed during the connection (e.g.: port=0)
                this._bindedEndpoint = (IPEndPoint) this._serverSocket.LocalEndPoint;

                // Log the action
                Console.WriteLine("Now listening on {0}...", this.GetListeningEndpoint());

                // Process every incoming messages for ever
                while (true) {
                    this.ProcessMessages();
                }
            }
            catch (SocketException exc) {
                Console.WriteLine(exc.Message);
            }
            catch (ObjectDisposedException) {
                Console.WriteLine("{0} server connection was closed...", this.GetListeningEndpoint().Port);
            }
            finally {
                // Finally, close the server socking that we were listening on
                this._serverSocket.Close();

                // Make the room unregistered (inactive)
                SERVER_ROOMS.Remove(this);
            }
        }
    }
}
