using System.Net;

namespace Shared {
    public static class IPUtils {
        /// <summary>
        /// Attempt to convert a string containing an IP address
        /// and an optional port to a IP address endpoint.
        /// </summary>
        ///
        /// <param name="inputEndpoint">The input string to parse.</param>
        /// <param name="parsedEndPoint">
        /// The result output.
        /// Will be <c>null</c> if <see cref="inputEndpoint"/> is invalid.
        /// </param>
        ///
        /// <returns>
        /// <c>False</c> is the string was containing invalid data,
        /// <c>True</c> if the parsing was successful.
        /// </returns>
        public static bool TryParseEndpoint(string inputEndpoint, out IPEndPoint parsedEndPoint) {
            // Split the port from the input string, if it's provided.
            var receivedEndpoint = inputEndpoint.Split(':', 2);

            // Default the endpoint port to the default config port.
            var port = DefaultConfig.DEFAULT_SERVER_PORT;

            // Attempt to parse the port if provided.
            var portIsInvalid = receivedEndpoint.Length > 1 && !short.TryParse(receivedEndpoint[1], out port);

            // If the port is invalid or the IP address is invalid,
            // abort and report the error.
            if (portIsInvalid || !IPAddress.TryParse(receivedEndpoint[0], out var address)) {
                parsedEndPoint = null;
                return false;
            }

            // Everything is valid, create the endpoint.
            parsedEndPoint = new IPEndPoint(address, port);
            return true;
        }
    }
}
