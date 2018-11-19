using System;
using System.Collections.Generic;
using System.Net;
using Shared;
using CommandType = Shared.CommandType;

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
        /// The user's custom nickname (mandatory).
        /// </summary>
        private static string _nickname;

        /// <summary>
        /// The user's custom nickname (mandatory).
        /// </summary>
        private static DisposableClient _disposableClient;

        /// <summary>
        /// Register the events that gets asynchronously triggered.
        ///
        /// <list type="bullet">
        ///     <item>
        ///         <term><c>CancelKeyPress</c></term>
        ///         <description>
        ///         Whenever the user hits <tt>CTRL+C</tt>, dispatching a <c>Dispose</c>
        ///         event to the <see cref="_disposableClient"/>, to clean up everything (thread, and sockets).
        ///         Then, when everything is clean, exits with <tt>1</tt>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><c>MessageReceivedEvent</c></term>
        ///         <description>
        ///         Whenever a message was received from the targeted chat (see <see cref="ParseArgs"/>)
        ///         server, it prints it to <tt>stdout</tt>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </summary>
        private static void RegisterEvents() {
            // .NET does not dispatch the termination event to disposables and finally blocks,
            // thus, we manually tell .NET not to handle the termination and just dispatch it
            // to the disposable that will handle all the cleaning, and then exit.
            Console.CancelKeyPress += (sender, eventArgs) => {
                _disposableClient?.Dispose();
                eventArgs.Cancel = true;
                Environment.Exit(1);
            };

            // Log each received message to stdout
            _disposableClient.MessageReceivedEvent += (receivedMessage, remoteEndpoint) => {
                Console.WriteLine(Environment.NewLine + receivedMessage);
            };
        }

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
        /// Prompt the user to input a message, for ever.
        /// Each message submitted by the user is sent to a given server (more info: <see cref="Main"/>),
        /// and logged into <tt>stdout</tt>.
        /// </summary>
        private static void PromptForMessageForEver() {
            // While the client is active, wait for the user's input
            while (_disposableClient.IsActive) {
                // Prompt the user, what message and command to send to the server
                var chatMessage = PromptAllFields();

                // Send the message to the server
                _disposableClient.SendMessage(chatMessage);

                // Log the message to stdout
                Console.WriteLine(chatMessage);
            }
        }

        /// <summary>
        /// Parse the arguments that the application received.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="serverEndpoint">The parsed server's <see cref="IPEndPoint"/></param>
        public static void ParseArgs(IReadOnlyList<string> args, out IPEndPoint serverEndpoint) {
            serverEndpoint = null;

            if (args.Count == 0) {
                serverEndpoint = DefaultConfig.GetDefaultEndPoint();
            }
            else if (args.Count > 1) {
                // An invalid argument count was passed, it's an error.
                PrintUsage(isAnError: true);
            }
            else if (args[0] == HELP_ARGUMENT) {
                // The user requested help, show it.
                PrintUsage(isAnError: false);
            }
            else if (!IPUtils.TryParseEndpoint(args[0], out serverEndpoint)) {
                // The user passed `host[:port]` and it was unsuccessfully parsed.
                PrintUsage(isAnError: true);
            }
        }

        /// <summary>
        /// Prompt a given message and ensure it's not longer than the maximal length accepted.
        /// </summary>
        /// <param name="promptMessage">The message to prompt.</param>
        /// <param name="maximalLength">The maximal length.</param>
        /// <returns></returns>
        public static string Prompt(string promptMessage, int maximalLength) {
            var readString = string.Empty;

            // Prompt the user until the message is no longer empty, or non longer too long
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
                // Prompt for a single key
                Console.Write("Command (POST = 0, GET = 1, SUB = 5, and UNSUB = 7): ");
                var inputCommand = Console.ReadKey().KeyChar.ToString();

                // Return at a new line
                Console.WriteLine();

                // Attempt to parse it
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
        /// The entry point of the UDP chat client. It starts a connection to a given server
        /// (see <see cref="ParseArgs"/>), and then, whenever everything is safe (see <see cref="RegisterEvents"/>),
        /// it waits for the user to send messages.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args) {
            // Parse passed argument values
            ParseArgs(args, out var serverEndpoint);

            // Prompt for a nickname
            _nickname = Prompt(
                "Nickname: ", ChatMessage.MAX_NICKNAME_SIZE - 1); // The maximal nickname length, excluding NUL

            using (_disposableClient = new DisposableClient(serverEndpoint)) {
                // Register every async events to the client before
                // entering the blocking mode
                RegisterEvents();

                // Prompt the user to send a message, forever
                PromptForMessageForEver();
            }
        }
    }
}
