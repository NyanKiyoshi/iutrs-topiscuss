using System.Data;
using System.Net;
using System.Net.Sockets;

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

        /// <summary>
        /// Waits for a <see cref="ChatMessage"/> message on a given socket,
        /// and return the sender's endpoint to a out parameter
        /// (we do not want to override any existing variable with possible
        /// a malicious sender's endpoint to take over the listened server).
        /// </summary>
        /// <param name="sourceSocket">The socket to read from.</param>
        /// <param name="remoteEndPoint">The sender's endpoint.</param>
        /// <returns>The received and parsed chat message.</returns>
        /// <exception cref="SyntaxErrorException">If the received byte buffer is invalid.</exception>
        public static ChatMessage ReceiveMessage(Socket sourceSocket, out EndPoint remoteEndPoint) {
            // Create a IP address endpoint to store the client information into
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Wait for a message and retrieve it
            var buffer = new byte[ChatMessage.FINAL_BUFFER_MAX_SIZE];
            sourceSocket.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref remoteEndPoint);

            // Decode the received buffer and return it
            return new ChatMessage(buffer);
        }
    }
}
