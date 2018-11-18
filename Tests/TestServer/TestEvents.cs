using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private StringWriter _textWriter;

        /// <summary>
        /// Generate a valid loopback endpoint,
        /// but with a different memory address each time.
        /// </summary>
        /// <returns>The endpoint</returns>
        private static IPEndPoint TestEndPoint() {
            return new IPEndPoint(IPAddress.Loopback, ushort.MaxValue);
        }

        /// <summary>
        /// Converts <see cref="_textWriter"/> to a <see cref="String"/>
        /// and strip any whitespace.
        /// </summary>
        /// <returns>The stripped <see cref="_textWriter"/>.</returns>
        private string ReadTextWritter() {
            return this._textWriter.ToString().Trim();
        }

        /// <summary>
        /// Store a dummy message.
        /// </summary>
        [SetUp]
        public void Setup() {
            // Set-up a custom stdout to mock it
            this._textWriter = new StringWriter();
            Console.SetOut(this._textWriter);

            // The message to store
            this._chatMessage = new ChatMessage(
                Command.POST, CommandType.RESPONSE, "Nick's name", "Hello world! :-)");

            // Simulate the POST command, storing the message
            ServerProgram.handle_POST(
                this._chatMessage, new IPEndPoint(IPAddress.Any, 0));

            // Clear off the mocked text writer
            this._textWriter.GetStringBuilder().Clear();
        }

        /// <summary>
        /// Remove chat messages from the server at tear down time.
        /// </summary>
        [TearDown]
        public void TearDown() {
            ServerProgram.STORED_CHAT_MESSAGES.Clear();
            ServerProgram.SUBSCRIBERS.Clear();
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
            Assert.AreEqual(ServerProgram.STORED_CHAT_MESSAGES.Count, 1);

            // Simulate the GET command, get the message
            ServerProgram.handle_GET(null, TestEndPoint());
        }

        /// <summary>
        /// Ensure the <see cref="ServerProgram.handle_SUB"/>
        /// is correctly registering users, and the endpoints are kept unique
        /// even if the objects are not of the same address.
        /// </summary>
        [TestCase]
        public void Test_handle_SUB() {
            // Ensure the subscribers list is empty
            // and no data was written to the mocked console
            Assert.AreEqual(ServerProgram.SUBSCRIBERS.Count, 0);
            Assert.IsEmpty(this._textWriter.ToString());

            // Simulate the SUB command
            ServerProgram.handle_SUB(null, TestEndPoint());

            // Ensure the subscription alert was written to the console
            Assert.That(this.ReadTextWritter(), Does.EndWith(" just subscribed!"));
            this._textWriter.GetStringBuilder().Clear();

            // Ensure it fails to subscribe twice the same endpoint data
            ServerProgram.handle_SUB(null, TestEndPoint());
            Assert.IsEmpty(this._textWriter.ToString());

            // Check if we are now subscribed
            Assert.AreEqual(ServerProgram.SUBSCRIBERS.Count, 1);
            CollectionAssert.AreEqual(
                ServerProgram.SUBSCRIBERS, new HashSet<EndPoint> {TestEndPoint()});
        }

        /// <summary>
        /// Ensure the <see cref="ServerProgram.handle_UNSUB"/>
        /// is correctly unregistering users.
        /// </summary>
        [TestCase]
        public void Test_handle_UNSUB() {
            // Ensure the subscribers list is empty
            // and no data was written to the mocked console
            Assert.AreEqual(ServerProgram.SUBSCRIBERS.Count, 0);
            Assert.AreEqual(ServerProgram.SUBSCRIBERS.Count, 0);
            Assert.IsEmpty(this._textWriter.ToString());

            // Simulate the SUB command to add a dummy subscriber
            ServerProgram.handle_SUB(null, TestEndPoint());
            Assert.AreEqual(ServerProgram.SUBSCRIBERS.Count, 1);

            // Ensure the alert was written to the console
            Assert.That(this.ReadTextWritter(), Does.EndWith(" just subscribed!"));

            // Try to unsubscribe
            ServerProgram.handle_UNSUB(null, TestEndPoint());
            Assert.AreEqual(ServerProgram.SUBSCRIBERS.Count, 0);

            // Ensure the alert was written to the console
            Assert.That(this.ReadTextWritter(), Does.EndWith(" just unsubscribed!"));
        }

        /// <summary>
        /// Ensure the <see cref="ServerProgram.handle_UNSUB"/>
        /// does not crash when requested to unsubscribe a non subscriber.
        /// </summary>
        [TestCase]
        public void Test_handle_UNSUB_withoutBeingSubscribed() {
            // Ensure the subscribers list is empty
            // and no data was written to the mocked console
            Assert.AreEqual(ServerProgram.SUBSCRIBERS.Count, 0);
            Assert.IsEmpty(this._textWriter.ToString());

            // Try to unsubscribe a non subscribed endpoint
            ServerProgram.handle_UNSUB(null, TestEndPoint());

            // Ensure no alert was written to the console,
            // meaning nothing was done, as expected
            Assert.IsEmpty(this._textWriter.ToString());
        }
    }
}
