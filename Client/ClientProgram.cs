﻿using System;
using System.Collections.Generic;
using System.Net;
using Shared;

namespace Client {
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
        /// The entry point of the UDP chat client.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args) {
            if (args.Length > 0) {
                ParseArgs(args);
            }

            Console.WriteLine("Hello World!");
        }
    }
}