using System.Text;
using NUnit.Framework;
using Shared;

namespace Tests.TestShared {
    public class TestByteUtils {
        /// <summary>
        /// Test <see cref="ByteUtils.GetNulTerminatedString"/> against valid buffers to check
        /// if it's correctly handling every case of mixed count vs start pos, including empty results.
        ///
        /// It also ensures that the pointer is correctly returned as being
        /// at the position of the <code>NUL</code> termination.
        /// </summary>
        ///
        /// <param name="startIndex">The index from which it will need to start looking from.</param>
        /// <param name="maxCount">The maximum count of chars to looks at.</param>
        /// <param name="inputString">The input string to convert to an array of bytes to extract from.</param>
        /// <param name="expectedStringResult">The expected string that we should get from the function.</param>
        [TestCase(0, 12, "Hello world\0", "Hello world")]
        [TestCase(0, 12, "Hello worl\0d", "Hello worl")]
        [TestCase(1, 12, "Hello worl\0d", "ello worl")]
        [TestCase(1, 12, "H\0d", "")]
        [TestCase(0, 12, "\0d", "")]
        public void Test_GetNulTerminatedString_ValidData(
                int startIndex, int maxCount,
                string inputString, string expectedStringResult) {

            // Convert the input string to an array of bytes
            var inputBuffer = Encoding.ASCII.GetBytes(inputString);

            // Get the expected end position, which is equal to the position of the first NUL
            var expectedEndIndex = (ushort)inputString.IndexOf('\0');

            // Test if the end index was the one expected (at `\0`)
            Assert.AreEqual(
                expectedEndIndex,
                ByteUtils.GetNulTerminatedString(
                    buffer: inputBuffer,
                    startIndex: (ushort)startIndex,
                    maxCount: (ushort)maxCount,
                    foundString: out var foundString
                )
            );

            // Test if the found string is was we expected to get (before `\0`)
            Assert.AreEqual(foundString, expectedStringResult);
        }

        /// <summary>
        /// Test <see cref="ByteUtils.GetNulTerminatedString"/> against invalid buffers to check
        /// if it's correctly handling the cases of wrong data being received.
        /// </summary>
        ///
        /// <param name="startIndex">The index from which it will need to start looking from.</param>
        /// <param name="maxCount">The maximum count of chars to looks at.</param>
        /// <param name="inputString">The input string to convert to an array of bytes to extract from.</param>
        [TestCase(0, 11, "Hello world\0")]
        [TestCase(0, 101, "Hello world :)")]
        [TestCase(0, 11, "Hello world")]
        [TestCase(0, 5, "Hello world")]
        [TestCase(0, 10, "Hello worl\0d")]
        [TestCase(1, 12, "\0")]
        [TestCase(1, 12, "\0Hd")]
        [TestCase(0, 1, "d\0")]
        public void Test_GetNulTerminatedString_InvalidData(int startIndex, int maxCount, string inputString) {
            // Convert the input string to an array of bytes
            var inputBuffer = Encoding.ASCII.GetBytes(inputString);

            // Test if the end index was the one expected (at `\0`)
            Assert.Throws<NULTerminationNotFound>(
                () => ByteUtils.GetNulTerminatedString(
                        buffer: inputBuffer,
                        startIndex: (ushort)startIndex,
                        maxCount: (ushort)maxCount,
                        foundString: out _
                )
            );
        }
    }
}
