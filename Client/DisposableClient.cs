using System;
using System.Data;
using System.Linq;
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
        /// The polling time (in <tt>us</tt>) on the <see cref="ClientSocket"/>
        /// </summary>
        private const int POLLING_TIME_MICROSECONDS = 20000;

        /// <summary>
        /// The list of commands, formatted as below.
        /// <code>
        /// \n\t{COMMAND NAME} ({COMMAND DECIMAL VALUE})
        /// ...
        /// </code>
        /// </summary>
        private static readonly string COMMAND_LIST =
            string.Join(
                '\n',
                Enum.GetValues(typeof(Command))
                    .Cast<Command>()
                    .ToList()
                    .ConvertAll(
                        command => string.Format(
                            "\t{0} ({1})",
                            command.ToString("G"),
                            command.ToString("D")
                        )
                    )
            );

        /// <summary>
        /// The client's forever message listening task.
        /// </summary>
        private Thread _messageListeningThread;

        /// <summary>
        /// The server endpoint to which we want to send data.
        /// </summary>
        private IPEndPoint _serverEndpoint;

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
            this._serverEndpoint = serverEndpoint;
            this.Start();
        }

        /// <summary>
        /// Logs a given formatted message into the server's stdout.
        /// </summary>
        /// <param name="format">The <see cref="String"/> message to format.</param>
        /// <param name="arg">The formatting arguments.</param>
        public static void LogInfo(string format, params object[] arg) {
            Console.Error.WriteLine("[Client INFO] " + "[" + DateTime.Now + "] " + format, arg: arg);
        }

        /// <summary>
        /// Start a disposable client listening on a given server endpoint (<see cref="_serverEndpoint"/>)
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
            switch (chatMessage.Command) {
                case Command.CONNECT:
                    if (IPUtils.TryParseEndpoint(chatMessage.Data, out var parsedEndpoint)) {
                        this._serverEndpoint = parsedEndpoint;
                        LogInfo("Server is now {0}.", parsedEndpoint);
                    }
                    else {
                        LogInfo("{0} is an invalid endpoint, CONNECT aborted.", chatMessage.Data);
                    }
                    break;
                case Command.HELP:
                    Console.WriteLine(COMMAND_LIST);
                    break;
                default:
                    IPUtils.SendMessage(this.ClientSocket, chatMessage, this._serverEndpoint);
                    break;
            }
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
