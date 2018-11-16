using System.Data;
using NUnit.Framework;
using Shared;
using CommandType = Shared.CommandType;

namespace Tests.TestShared {
    public class TestChatMessage {
        /// <summary>
        /// Ensure that passing invalid <see cref="Command"/> or <see cref="Shared.CommandType"/>
        /// does not raise any exception.
        /// </summary>
        /// <param name="bufferToDecode">The bytes to attempt to decode.</param>
        [TestCase(arg: new byte[] {0xFF, 0xFE, 0x48, 0, 0x48, 0})]
        public void Test_ChatMessage_DeserializeInvalidCommandData(byte[] bufferToDecode) {
            var result = new ChatMessage(dataBuffer: bufferToDecode).ToString();
            Assert.AreEqual(result, "[254][255][1] H: H");
        }

        /// <summary>
        /// Test <see cref="ChatMessage"/> deserialization again too short byte array.
        /// </summary>
        /// <param name="bufferToDecode">The bytes to attempt to decode.</param>
        [TestCase(arg: new byte[] {0xFF, 0xFE, 0})]
        [TestCase(arg: new byte[] {})]
        public void Test_ChatMessage_DeserializeInvalidSize(byte[] bufferToDecode) {
            Assert.Throws<InvalidBufferLength>(() => {
                var _ = new ChatMessage(dataBuffer: bufferToDecode);
            });
        }

        /// <summary>
        /// Test <see cref="ChatMessage"/> deserialization again a too big byte array.
        /// </summary>
        [TestCase]
        public void Test_ChatMessage_DeserializeTooBig() {
            var arr = new byte[ChatMessage.FINAL_BUFFER_MAX_SIZE + 1];

            Assert.Throws<InvalidBufferLength>(() => {
                var _ = new ChatMessage(dataBuffer: arr);
            });
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

            // Ensure the deserialization returns the exact same data
            var newChatMessage = new ChatMessage(buffer);
            Assert.AreEqual(chatMessage.Command, newChatMessage.Command);
            Assert.AreEqual(chatMessage.CommandType, newChatMessage.CommandType);
            Assert.AreEqual(chatMessage.Nickname, newChatMessage.Nickname);
            Assert.AreEqual(chatMessage.Data, newChatMessage.Data);

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
