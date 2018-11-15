using System.Net;

namespace Shared {
    public static class DefaultConfig {
        public const short DEFAULT_SERVER_PORT = 6000;
        public static readonly IPAddress DEFAULT_SERVER_HOST = IPAddress.Loopback;
    }
}
