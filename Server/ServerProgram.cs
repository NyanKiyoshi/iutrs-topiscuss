using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Shared;

namespace Server {
    /// <summary>
    /// The UDP chat's server entry point.
    /// </summary>
    internal static class ServerProgram {
        /// <summary>
        /// The signature of a method capable of handling a dispatched event
        /// coming from a given endpoint.
        /// </summary>
        /// <param name="receivedMessage">The message that we received and that needs to be handled.</param>
        /// <param name="clientEndPoint">The remote endpoint from whom the message came from.</param>
        private delegate void CommandHandler(ChatMessage receivedMessage, EndPoint clientEndPoint);

        /// <summary>
        /// The list of commands supported by this server and their handler.
        ///
        /// <code>
        ///     COMMAND => ServerProgram.handle_COMMAND
        /// </code>
        /// </summary>
        private static readonly Dictionary<Command, CommandHandler> COMMAND_DISPATCHERS =
            new Dictionary<Command, CommandHandler> {
                {Command.GET, handle_GET},
                {Command.POST, handle_POST}
            };

        /// <summary>
        /// The list of stored messages, which gets populated by <see cref="Command.GET"/>,
        /// in other words, <see cref="handle_POST"/>.
        /// </summary>
        private static readonly List<ChatMessage> STORED_CHAT_MESSAGES = new List<ChatMessage>();

        /// <summary>
        /// Logs a given formatted message into the server's stdout.
        /// </summary>
        /// <param name="format">The <see cref="String"/> message to format.</param>
        /// <param name="arg">The formatting arguments.</param>
        private static void LogInfo(string format, params object[] arg) {
            Console.WriteLine("[Server INFO] " + "[" + DateTime.Now + "] " + format, arg: arg);
        }

        /// <summary>
        /// Handles a <see cref="Command.GET"/> request,
        /// sending every <see cref="STORED_CHAT_MESSAGES">stored chat messages</see>
        /// to the requesting client.
        /// </summary>
        /// <param name="receivedMessage">The message that requested</param>
        /// <param name="clientEndPoint">The endpoint that requested our stored data.</param>
        private static void handle_GET(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Send every stored message to the client
            foreach (var storedChatMessage in STORED_CHAT_MESSAGES) {
                // Convert the message to a byte buffer
                var bufferToSend = storedChatMessage.GetBytes();

                // Send the message
                var sentBytes = _serverSocket.SendTo(
                    buffer: bufferToSend,
                    offset: 0,
                    size: bufferToSend.Length,
                    socketFlags: SocketFlags.None,
                    remoteEP: clientEndPoint);

                // Log the fact we just sent data out
                LogInfo("Sent {0} bytes to {1}", sentBytes, clientEndPoint);
            }
        }

        /// <summary>
        /// Handle the a <see cref="Command.POST"/> request,
        /// sending all stored message to the requester.
        /// </summary>
        /// <param name="receivedMessage">The message received.</param>
        /// <param name="clientEndPoint">The remote sender's endpoint.</param>
        private static void handle_POST(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Change the command type to response,
            // as we will only use it from `handle_GET` as response a message.
            receivedMessage.CommandType = CommandType.RESPONSE;

            // Store the message
            STORED_CHAT_MESSAGES.Add(receivedMessage);

            // Log what we just did
            LogInfo(
                "{0} just stored a {1} characters-long message",
                clientEndPoint, receivedMessage.Data.Length);
        }

        /// <summary>
        /// The server socket from which we will bind ourselves and listen for incoming messages.
        /// </summary>
        private static Socket _serverSocket;

        /// <summary>
        /// Handle a received message from a given client.
        /// </summary>
        /// <param name="chatMessage">The message to handle.</param>
        /// <param name="clientEndPoint">The client endpoint that the message came from.</param>
        private static void HandleMessage(ChatMessage chatMessage, EndPoint clientEndPoint) {
            // Log the message
            LogInfo("{0}: {1}", clientEndPoint, chatMessage);

            // Dispatch the received command if it's not unknown
            if (COMMAND_DISPATCHERS.TryGetValue(chatMessage.Command, out var foundHandler)) {
                foundHandler(receivedMessage: chatMessage, clientEndPoint: clientEndPoint);
            }
        }

        /// <summary>
        /// Listens for messages and handle them.
        /// </summary>
        private static void ProcessMessages() {
            // Wait for a message, retrieve it and decode it
            var receivedMessage = IPUtils.ReceiveMessage(_serverSocket, out var clientEndPoint);

            try {
                // Handle the received message
                HandleMessage(receivedMessage, clientEndPoint);
            }
            catch (NULTerminationNotFound) {
                Console.WriteLine("Error: received invalid message.");
            }
        }

        /// <summary>
        /// Entry point for the UDP <see cref="Server"/>, it opens a socket
        /// and handles every incoming messages.
        /// </summary>
        private static void Main() {
            // Create the listening UDP socket
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try {
                // Bind the server socket to the default config
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, DefaultConfig.DEFAULT_SERVER_PORT));
                Console.WriteLine("Now listening on {0}...", _serverSocket.LocalEndPoint);

                // Process every incoming messages
                while (true) {
                    ProcessMessages();
                }
            }
            catch (SocketException exc) {
                Console.WriteLine(exc.Message);
            }
            finally {
                // Finally, close the server socking that we were listening on
                _serverSocket.Close();
            }
        }
    }
}
