using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shared;

namespace Client {
    /// <inheritdoc />
    /// <summary>
    /// A disposable chat client, that auto
    /// </summary>
    public class DisposableClient : IDisposable {
        /// <summary>
        /// The client's forever message listening task.
        /// </summary>
        private Thread _messageListeningThread;

        /// <summary>
        /// The polling time (in <tt>us</tt>) on the <see cref="ClientSocket"/>
        /// </summary>
        private const int POLLING_TIME_MICROSECONDS = 20000;

        /// <summary>
        /// The server endpoint to which we want to send data.
        /// </summary>
        public readonly IPEndPoint ServerEndpoint;

        /// <summary>
        /// The invoked event whenever a message is received from the server.
        /// </summary>
        ///
        /// <param name="chatMessage">
        /// The received message.
        /// </param>
        /// <param name="remoteEndpoint">
        /// The remote endpoint from where the message came (the server, supposedly).
        /// </param>
        public delegate void OnMessageReceived(ChatMessage chatMessage, EndPoint remoteEndpoint);

        /// <summary>
        /// The event that gets invoked whenever a message is received
        /// from the given server.
        /// </summary>
        public event OnMessageReceived MessageReceivedEvent;

        /// <summary>
        /// The client's state, whether it should be stopping or keep on running.
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// The client's socket that it's using to communicate with the server.
        /// Storing this value as a global will allow us to retrieve data from
        /// the server later on.
        /// </summary>
        public Socket ClientSocket;

        /// <summary>
        /// Initialize and start a disposable client that will listen on a given <see cref="serverEndpoint"/>.
        /// Then will get clean up everything through the <see cref="Dispose"/> method.
        /// </summary>
        /// <param name="serverEndpoint">The server's endpoint to be listened on for new messages.</param>
        public DisposableClient(IPEndPoint serverEndpoint) {
            this.ServerEndpoint = serverEndpoint;
            this.Start();
        }

        /// <summary>
        /// Start a disposable client listening on a given server endpoint (<see cref="ServerEndpoint"/>)
        /// for new messages in a separated thread.
        /// And send any provided messages using <see cref="SendMessage"/> to the server.
        /// </summary>
        private void Start() {
            this.ClientSocket =
                new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Linking the socket to the communication point
            this.ClientSocket.Bind(new IPEndPoint(IPAddress.Any, 0));

            // Check for messages and prompt the user what to do (continue or stop)
            this._messageListeningThread = new Thread(this.ReceiveMessagesForEver);
            this._messageListeningThread.Start();
        }

        /// <summary>
        /// Check for messages to be received,
        /// keep the method blocked for <see cref="POLLING_TIME_MICROSECONDS"/> microseconds.
        ///
        /// If a message was received, invoke the <see cref="MessageReceivedEvent"/>.
        /// </summary>
        private void WaitForMessage() {
            // Check if data is available to be received.
            // Stop looking for messages after a few given micro-seconds.
            while (this.ClientSocket.Poll(POLLING_TIME_MICROSECONDS, SelectMode.SelectRead)) {
                EndPoint remoteEndPoint = null;
                try {
                    // Wait for a message, retrieve it and decode it
                    var receivedMessage = IPUtils.ReceiveMessage(this.ClientSocket, out remoteEndPoint);

                    // Propagate the received message
                    this.MessageReceivedEvent?.Invoke(receivedMessage, remoteEndPoint);
                }
                catch (SyntaxErrorException) {
                    Console.WriteLine(
                        "Warning: received an invalid message from {0}", remoteEndPoint);
                }
                catch (SocketException) {
                    // Wait a little before retrying (in milliseconds)
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Attempt to receive messages from the server,
        /// will throw <see cref="SocketError"/> if it's unable to connect to the server.
        /// </summary>
        private void ReceiveMessagesForEver() {
            // While this task is needed, let it look for messages
            while (this.IsActive) {
                this.WaitForMessage();
            }

            // Log that the task just finished
            Console.Error.WriteLine("Stopped listening for new messages.");
        }

        /// <summary>
        /// Send a message to the server using the <see cref="ClientSocket"/>.
        /// </summary>
        /// <param name="chatMessage"></param>
        public void SendMessage(ChatMessage chatMessage) {
            IPUtils.SendMessage(this.ClientSocket, chatMessage, this.ServerEndpoint);
        }

        /// <inheritdoc />
        /// <summary>
        /// Flag the <see cref="IDisposable"/> implementation
        /// as to stop listening on the server for new messages
        /// and wait for the sub thread(s) to handle the exit flag.
        /// Then, close the <see cref="ClientSocket"/>.
        /// </summary>
        public void Dispose() {
            // Flag the application to stop itself
            this.IsActive = false;
            Console.Error.WriteLine();

            // Wait for the thread to stop listening for new messages
            if (this._messageListeningThread.IsAlive) {
                Console.Error.WriteLine("Waiting for the client to stop listening...");
                this._messageListeningThread.Join();
            }

            // Close the socket
            Console.Error.WriteLine("Closing the connection...");
            this.ClientSocket.Close();
        }
    }
}
