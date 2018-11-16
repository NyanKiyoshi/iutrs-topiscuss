using NUnit.Framework;
using Shared;

namespace Tests.TestShared {
    public class TestChatMessage {
        /// <summary>
        /// Ensure that passing invalid <see cref="Command"/> or <see cref="CommandType"/>
        /// does not raise any exception.
        /// </summary>
        /// <param name="bufferToDecode">The bytes to attempt to decode.</param>
        [TestCase(arg: new byte[] {0xFF, 0xFE, 0x48, 0, 0x48, 0})]
        public void Test_ChatMessage_DeserializeInvalidCommandData(byte[] bufferToDecode) {
            var result = new ChatMessage(dataBuffer: bufferToDecode).ToString();
            Assert.AreEqual(result, "[254][255][1] H: H");
        }

        /// <summary>
        /// Ensures <see cref="ChatMessage.GetBytes"/> correctly transforms given data
        /// to bytes.
        ///
        /// Note: it's not its job to check the lengths.
        /// </summary>
        [TestCase]
        public void Test_GetBytes_WithValidData() {
            // Create a dummy message
            var chatMessage = new ChatMessage(Command.GET, CommandType.RESPONSE, "Hi", "Data");

            // Convert the message to bytes
            var buffer = chatMessage.GetBytes();

            // Check the buffer is what we expected
            Assert.AreEqual(
                buffer,
                new byte[] {
                    // header
                    (byte)Command.GET,
                    (byte)CommandType.RESPONSE,

                    // nickname + NUL
                    72, 105, byte.MinValue,

                    // data + NUL
                    68, 97, 116, 97, byte.MinValue
            });

            // Ensure the deserialization returns the exact same data
            var newChatMessage = new ChatMessage(buffer);
            Assert.AreEqual(chatMessage.Command, newChatMessage.Command);
            Assert.AreEqual(chatMessage.CommandType, newChatMessage.CommandType);
            Assert.AreEqual(chatMessage.Nickname, newChatMessage.Nickname);
            Assert.AreEqual(chatMessage.Data, newChatMessage.Data);
        }

        /// <summary>
        /// Test the <see cref="ChatMessage.ToString"/> method is returning what we expect.
        /// </summary>
        [TestCase]
        public void Test_ToString_WithValidData() {
            // Create a dummy message
            var chatMessage = new ChatMessage(Command.GET, CommandType.RESPONSE, "Hi", "Data");
            Assert.AreEqual(chatMessage.ToString(), "[RESPONSE][GET][4] Hi: Data");
        }
    }
}
