using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Shared;

namespace Client {
    /// <summary>
    /// The UDP chat client's entry point class.
    /// </summary>
    public static class ClientProgram {
        /// <summary>
        /// The parameter representing a help request.
        /// </summary>
        public const string HELP_ARGUMENT = "--help";

        /// <summary>
        /// The server endpoint to which we want to send data.
        /// This initialize a the default server config, it may get overriden by the user in
        /// <see cref="ParseArgs"/>.
        ///
        /// For more information, see: <see cref="PrintUsage"/>.
        /// </summary>
        private static IPEndPoint _serverEndpoint = new IPEndPoint(
            DefaultConfig.DEFAULT_SERVER_HOST, DefaultConfig.DEFAULT_SERVER_PORT);

        /// <summary>
        /// The client's socket that it's using to communicate with the server.
        /// Storing this value as a global will allow us to retrieve data from
        /// the server later on.
        /// </summary>
        private static Socket _clientSocket;

        /// <summary>
        /// The user's custom nickname (mandatory).
        /// </summary>
        private static string _nickname;

        /// <summary>
        /// Prints the usage of this program.
        /// </summary>
        /// <param name="isAnError">
        /// Is this print an error? If so, it will write to stderr and exit with a non-zero code (1).</param>
        private static void PrintUsage(bool isAnError) {
            // If it's an error, write to `stderr`. Else, `stdout`.
            var outputFile = isAnError ? Console.Error : Console.Out;

            // Write the help
            outputFile.WriteLine(
                "Usage[1]: {0} [SERVER_IP[:SERVER_PORT]]\r\n" +
                "Usage[2]: {0} [{1}]",
                AppDomain.CurrentDomain.FriendlyName, HELP_ARGUMENT);

            // Exit with `1` if it's an error, or 0 if it's was a requested help.
            ControlledEnvironment.Current.Exit(isAnError ? 1 : 0);
        }

        /// <summary>
        /// Parse the arguments that the application received.
        /// It's expecting a non empty collection of string.
        /// </summary>
        /// <param name="args"></param>
        public static void ParseArgs(IReadOnlyList<string> args) {
            if (args.Count != 1) {
                // An invalid argument count was passed, it's an error.
                PrintUsage(isAnError: true);
            }
            else if (args[0] == HELP_ARGUMENT) {
                // The user requested help, show it.
                PrintUsage(isAnError: false);
            }
            else if (!IPUtils.TryParseEndpoint(args[0], out _serverEndpoint)) {
                // The user passed `host[:port]` and it was unsuccessfully parsed.
                PrintUsage(isAnError: true);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promptMessage"></param>
        /// <param name="maximalLength"></param>
        /// <returns></returns>
        public static string Prompt(string promptMessage, int maximalLength) {
            var readString = string.Empty;

            while (readString?.Length < 1 || readString?.Length > maximalLength) {
                Console.Write(promptMessage);
                readString = Console.ReadLine()?.Trim();
            }

            return readString;
        }

        /// <summary>
        /// Prompt the user to enter a "valid" <see cref="Command"/>.
        ///
        /// Note that a non existing <see cref="Command"/> <see cref="Int32"/> value is still
        /// taken as a valid value. But only a bad <see cref="String"/> name is detected.
        ///
        /// If a <see cref="Int32"/> value is passed, it will be cased later on by
        /// <see cref="ChatMessage"/> to a <see cref="Byte"/>.
        /// </summary>
        ///
        /// <returns>The submitted command.</returns>
        public static Command PromptCommand() {
            while (true) {
                var inputCommand = Prompt("Command (POST [0], GET [1], SUB [5] or UNSUB [6]): ", 10).ToUpper();

                if (Enum.TryParse(inputCommand, out Command foundCommand)) {
                    return foundCommand;
                }
            }
        }

        /// <summary>
        /// Prompt the <see cref="Command"/> and <c>message</c> (<see cref="String"/>)
        /// to be sent to the server (using <see cref="ChatMessage"/>'s message serialization).
        /// </summary>
        /// <returns>The built message to be sent, from the user input.</returns>
        public static ChatMessage PromptAllFields() {
            var command = PromptCommand();
            var message = Prompt("Message: ", ChatMessage.MAX_DATA_SIZE - 1);  // minus NUL

            // Returning the resulting chat message, from the user input
            return new ChatMessage(
                command: command,
                type: CommandType.REQUEST,
                nickname: _nickname,
                data: message);
        }

        /// <summary>
        /// The entry point of the UDP chat client.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args) {
            // If there are any passed argument values, parse them.
            if (args.Length > 0) {
                ParseArgs(args);
            }

            // Prompt for a nickname
            _nickname = Prompt(
                "Nickname: ", ChatMessage.MAX_NICKNAME_SIZE - 1);  // The maximal nickname length, excluding NUL

            // Log the server endpoint that we are going to use,
            // and prepare a UDP socket to use to send message to the server.
            Console.WriteLine("Using {0}", _serverEndpoint);
            _clientSocket =
                new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try {
                while (true) {
                    // Prompt the user, what message and command to send to the server
                    var chatMessage = PromptAllFields();

                    // Convert this message to bytes
                    var buffer = chatMessage.GetBytes();

                    // Send the buffer to the server
                    _clientSocket.SendTo(
                        buffer, 0, buffer.Length, SocketFlags.None, _serverEndpoint);

                    // Log the message to stdout
                    Console.WriteLine(chatMessage);

                    // If the command was a request,
                    // wait for a response from the server
                    if (chatMessage.Command == Command.GET) {
                        // Check if data is available to be received.
                        // Stop looking for messages after 1ms.
                        while (_clientSocket.Poll(10000, SelectMode.SelectRead)) {
                            var receivedMessage = IPUtils.ReceiveMessage(_clientSocket, out var remoteEndPoint);
                            Console.WriteLine(receivedMessage);
                        }
                    }
                }
            }
            finally {
                // Finally, close the socket
                _clientSocket.Close();
            }
        }
    }
}
