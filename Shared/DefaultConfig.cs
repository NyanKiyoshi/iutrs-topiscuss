using System.Net;

namespace Shared {
    /// <summary>
    /// The default client and server shared config.
    /// </summary>
    public static class DefaultConfig {
        public const ushort DEFAULT_SERVER_PORT = 6000;
        public static readonly IPAddress DEFAULT_SERVER_HOST = IPAddress.Loopback;

        /// <summary>
        /// Returns the default server's endpoint config.
        /// </summary>
        /// <returns>The default server's endpoint.</returns>
        public static IPEndPoint GetDefaultEndPoint() {
            return new IPEndPoint(DEFAULT_SERVER_HOST, DEFAULT_SERVER_PORT);
        }
    }
}
