using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using Client.views;
using Shared;
using Terminal.Gui;

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

        public static void PromptNickname() {
            var nickname = new InputBox(
                "Customize your Nickname",
                "Nickname",
                minLength: 2,
                maxLength: ChatMessage.MAX_NICKNAME_SIZE,
                onSuccess: (_, __) => {}).TextInput;
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
            Application.Init();

            var window = new Window(Environment.CommandLine);

            var container = new TextView();
            var stringBuilder = new StringBuilder(string.Empty);
            container.CanFocus = false;
            container.Height = Dim.Fill() - 2;
            container.Text = string.Empty;

            Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(0.1),
                loop => {
                    stringBuilder.Insert(0,
                        Environment.NewLine
                        + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    container.Text = stringBuilder.ToString();
                    return true;
                }
            );

            var inputView = new View();
            const string inputLabelText = "Text: ";
            inputView.Add(
                new Label(0, 0, inputLabelText),
                new TextField("") {
                    X = inputLabelText.Length + 2,
                    Width = Dim.Fill() - inputLabelText.Length,
                    Height = 2
                });

            inputView.X = 0;
            inputView.Height = 4;
            inputView.Width = Dim.Fill();

            container.Y = Pos.Bottom(inputView);

            window.Add(inputView, container);

            Application.Top.Add(window);
            Application.MainLoop.Invoke(() => {PromptNickname();});
            Application.Run(Application.Top);
        }
    }
}
