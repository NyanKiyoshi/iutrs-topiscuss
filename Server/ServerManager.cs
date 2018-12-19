using Shared;

namespace Server {
    public static class ServerManager {
        /// <summary>
        /// Entry point for the UDP server(s), it opens a socket
        /// and handles every incoming messages.
        /// </summary>
        private static void Main() {
            var baseServerRoom = new ServerRoom();
            baseServerRoom.Listen(DefaultConfig.DEFAULT_SERVER_PORT);
        }
    }
}
