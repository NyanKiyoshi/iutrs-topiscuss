using System.Data;
using System.Text;

namespace Shared {
    /// <inheritdoc />
    /// <summary>
    /// An exception that occurs when looking for a <c>NUL</c> termination, but none is found.
    /// </summary>
    public class NULTerminationNotFound : SyntaxErrorException {}

    public class ByteUtils {
        /// <summary>
        /// Parse a string from a given bytes buffer, starting from a given point
        /// until a <c>NUL</c> termination is found.
        /// Throws an exception if the max byte count was reached without finding any <c>NUL</c> terminated string.
        /// </summary>
        ///
        /// <param name="buffer">The buffer of bytes to read from.</param>
        /// <param name="startIndex">The index to start looking from.</param>
        /// <param name="maxCount">The maximum byte count to look into.</param>
        /// <param name="foundString">The variable to store the parsed string into, if successfully parsed.</param>
        ///
        /// <returns>
        /// The position of the <c>NUL</c> termination if any
        /// (<see cref="NULTerminationNotFound"/> is thrown otherwise).
        /// </returns>
        ///
        /// <exception cref="NULTerminationNotFound">
        /// Thrown if it was unable to find a <c>NUL</c> termination in the given range.
        /// </exception>
        public static ushort GetNulTerminatedString(
                byte[] buffer, ushort startIndex, ushort maxCount, out string foundString) {

            // Initialize the current count of bytes read to zero
            var currentCount = 0;

            // Initialize the cursor position to the given start index
            var currentPos = startIndex;

            // Look for a NUL termination in the given range,
            // meaning, until we reach the end of the buffer or, the maximal reading count was reached.
            while (currentCount < maxCount && currentPos < buffer.Length) {
                // If the current byte is not a NUL termination, there is nothing to do,
                // so just continue to looking through.
                if (buffer[currentPos] != byte.MinValue) {
                    ++currentPos;
                    ++currentCount;
                    continue;
                }

                // We found a NUL termination. Thus, we parse it to a string and stop here.
                foundString = Encoding.ASCII.GetString(buffer, startIndex, currentCount);
                return currentPos;
            }

            // We did not find a NUL termination in the given range,
            // it's, thus, an invalid buffer. Throw an exception.
            throw new NULTerminationNotFound();
        }
    }
}
