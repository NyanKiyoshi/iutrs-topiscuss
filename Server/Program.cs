using System;
using System.Net;
using System.Net.Sockets;
using Shared;

namespace Server {
    internal static class Program {
        /// <summary>
        /// The server socket from which we will bind ourselves and listen for incoming messages.
        /// </summary>
        private static Socket _serverSocket;

        /// <summary>
        /// Handle a received message from a given client.
        /// </summary>
        /// <param name="chatMessage">The message to handle.</param>
        /// <param name="clientEndpoint">The client endpoint that the message came from.</param>
        private static void HandleMessage(ChatMessage chatMessage, EndPoint clientEndpoint) {
            Console.WriteLine("[{0}]{1}", clientEndpoint, chatMessage);
        }

        /// <summary>
        /// Listens for messages and handle them.
        /// </summary>
        private static void ProcessMessages() {
            // Create a IP address endpoint to store the client information into
            EndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);

            // Wait for a message and retrieve it
            var buffer = new byte[ChatMessage.FINAL_BUFFER_MAX_SIZE];
            _serverSocket.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref clientEndpoint);

            try {
                // Decode the received buffer and handle the received message
                HandleMessage(new ChatMessage(buffer), clientEndpoint);
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
                _serverSocket.Close();
            }
        }
    }
}
