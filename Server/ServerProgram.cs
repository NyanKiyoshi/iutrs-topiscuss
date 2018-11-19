﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using Shared;
using CommandType = Shared.CommandType;

namespace Server {
    /// <summary>
    /// The UDP chat's server entry point.
    /// </summary>
    public static class ServerProgram {
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
                {Command.POST, handle_POST},
                {Command.SUB, handle_SUB},
                {Command.UNSUB, handle_UNSUB}
            };

        /// <summary>
        /// The list of stored messages, which gets populated by <see cref="Command.GET"/>,
        /// in other words, <see cref="handle_POST"/>.
        /// </summary>
        public static readonly List<ChatMessage> STORED_CHAT_MESSAGES = new List<ChatMessage>();

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
        public static readonly HashSet<EndPoint> SUBSCRIBERS = new HashSet<EndPoint>();

        /// <summary>
        /// Public setter for the server's <see cref="Socket"/> to listen
        /// and manipulate. This is useful to mock the socket in tests.
        /// </summary>
        public static Socket ServerSocket {
            set => _serverSocket = value;
        }

        /// <summary>
        /// Logs a given formatted message into the server's stdout.
        /// </summary>
        /// <param name="format">The <see cref="String"/> message to format.</param>
        /// <param name="arg">The formatting arguments.</param>
        private static void LogInfo(string format, params object[] arg) {
            Console.WriteLine("[Server INFO] " + "[" + DateTime.Now + "] " + format, arg: arg);
        }

        /// <summary>
        /// Sends a given <see cref="ChatMessage"/> to a given <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="chatMessage">The message to send.</param>
        /// <param name="remoteEndPoint">The endpoint to send to.</param>
        public static void SendMessage(ChatMessage chatMessage, EndPoint remoteEndPoint) {
            // Send the message
            var sentBytes = IPUtils.SendMessage(
                _serverSocket, chatMessage, remoteEndPoint);

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
        public static void handle_GET(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Send every stored message to the client
            foreach (var storedChatMessage in STORED_CHAT_MESSAGES) {
                SendMessage(storedChatMessage, clientEndPoint);
            }
        }

        /// <summary>
        /// Handle the a <see cref="Command.POST"/> request,
        /// storing the requester's message to the server,
        /// and sending it out to every subscriber.
        /// </summary>
        /// <param name="receivedMessage">The message received.</param>
        /// <param name="clientEndPoint">The remote sender's endpoint.</param>
        public static void handle_POST(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Change the command type to response,
            // as we will only use it from `handle_GET` as response a message.
            receivedMessage.CommandType = CommandType.RESPONSE;

            // Store the message
            STORED_CHAT_MESSAGES.Add(receivedMessage);

            // Log what we just did
            LogInfo(
                "{0} just stored a {1} characters-long message",
                clientEndPoint, receivedMessage.Data.Length);

            // Send the message to every subscriber
            foreach (var subscriberEndPoint in SUBSCRIBERS) {
                SendMessage(receivedMessage, subscriberEndPoint);
            }
        }

        /// <summary>
        /// Handle the a <see cref="Command.SUB"/> request,
        /// add the requester's endpoint to the subscribers list.
        /// </summary>
        /// <param name="receivedMessage">The message received.</param>
        /// <param name="clientEndPoint">The remote sender's endpoint.</param>
        public static void handle_SUB(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Add the user to subscribers list if not already subbed
            if (SUBSCRIBERS.Add(clientEndPoint)) {
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
        public static void handle_UNSUB(ChatMessage receivedMessage, EndPoint clientEndPoint) {
            // Attempt to remove the user from the subscribers list
            if (SUBSCRIBERS.Remove(clientEndPoint)) {
                // Log the new subscriber
                LogInfo(
                    "{0} just unsubscribed!", clientEndPoint);
            }
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
            EndPoint clientEndPoint = null;
            try {
                // Wait for a message, retrieve it and decode it
                var receivedMessage = IPUtils.ReceiveMessage(_serverSocket, out clientEndPoint);

                // Handle the received message
                HandleMessage(receivedMessage, clientEndPoint);
            }
            catch (SyntaxErrorException) {
                LogInfo("received an invalid message.");
            }
            catch (SocketException exc) {
                LogInfo(
                    "A connection with {0} was not properly ended. Reason: {1}",
                    clientEndPoint, exc.Message);
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
