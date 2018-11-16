using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using Server;
using Shared;

namespace Tests.TestServer {
    /// <inheritdoc />
    /// <summary>
    /// A mocked <see cref="T:System.Net.Sockets.Socket" /> to allow testing.
    /// </summary>
    internal class MockedSocket : Socket {
        /// <inheritdoc />
        /// <summary>
        /// A dummy mocked initializer for a <see cref="Socket"/>.
        /// </summary>
        public MockedSocket() : base(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {
        }
    }

    /// <summary>
    /// Tests the server events.
    /// </summary>
    public class TestEvents {
        private ChatMessage _chatMessage;

        /// <summary>
        /// Store a dummy message.
        /// </summary>
        [SetUp]
        public void Setup() {
            // The message to store
            this._chatMessage = new ChatMessage(
                Command.POST, CommandType.RESPONSE, "Nick's name", "Hello world! :-)");

            // Simulate the POST command, storing the message
            ServerProgram.handle_POST(
                this._chatMessage, new IPEndPoint(IPAddress.Any, 0));
        }

        /// <summary>
        /// Remove chat messages from the server at tear down time.
        /// </summary>
        [TearDown]
        public void TearDown() {
            ServerProgram.STORED_CHAT_MESSAGES.Clear();
        }

        /// <summary>
        /// Ensure the message from <see cref="Setup"/>'s call to
        /// <see cref="ServerProgram.handle_POST"/> was successfully handled.
        /// </summary>
        [TestCase]
        public void Test_handle_POST() {
            Assert.AreEqual(ServerProgram.STORED_CHAT_MESSAGES.Count, 1);
            Assert.AreEqual(ServerProgram.STORED_CHAT_MESSAGES[0], this._chatMessage);
        }

        /// <summary>
        /// Ensure the stored message by <see cref="Setup"/> is successfully
        /// sent to the requester' endpoint.
        /// </summary>
        [TestCase]
        public void Test_handle_GET() {
            // Initialize the test data and check the integrity
            ServerProgram.ServerSocket = new MockedSocket();
            var clientEndPoint = new IPEndPoint(IPAddress.Loopback, ushort.MaxValue);
            Assert.AreEqual(ServerProgram.STORED_CHAT_MESSAGES.Count, 1);

            // Simulate the GET command, get the message
            ServerProgram.handle_GET(null, clientEndPoint);
        }
    }
}
