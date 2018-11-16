using NUnit.Framework;
using Shared;

namespace Tests.TestShared {
    public class TestChatMessage {
        /// <summary>
        /// Ensure that passing invalid <see cref="Command"/> or <see cref="CommandType"/>
        /// does not raise any exception.
        /// </summary>
        /// <param name="bufferToDecode">The bytes to attempt to decode.</param>
        [TestCase(arg: new byte[] {0xFF, 0xFF})]
        public void Test_ChatMessage_DeserializeInvalidCommandData(byte[] bufferToDecode) {
            Assert.Pass(new ChatMessage(dataBuffer: bufferToDecode).ToString());
        }
    }
}
