using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Shared {
    public enum Command {POST, GET, HELP, QUIT, STOP, SUB, SUB2, UNSUB}
    public enum CommandType {REQUEST, RESPONSE}

    /// <inheritdoc />
    /// <summary>
    /// An exception occuring on invalid byte buffer length.
    /// </summary>
    public class InvalidBufferLength : SyntaxErrorException {
        /// <inheritdoc />
        /// <summary>
        /// A constructor that call the <see cref="T:System.Data.SyntaxErrorException" />'s constructor
        /// with a descriptive error message, containing the buffer length.
        /// </summary>
        /// <param name="buffer"></param>
        public InvalidBufferLength(IReadOnlyCollection<byte> buffer)
                : base(string.Format("Received an invalid length: {0}", buffer.Count)) {
            // noop
        }
    }

    public class ChatMessage {
        /// <summary>
        /// The maximal size allowed for a nickname to be sent or be received.
        /// </summary>
        /// <seealso cref="Shared.CommandType"/>
        public const int MAX_NICKNAME_SIZE = 30;

        /// <summary>
        /// The maximal data size allowed to be sent or be received.
        /// </summary>
        /// <seealso cref="Shared.CommandType"/>
        public const int MAX_DATA_SIZE = 2000;

        /// <summary>
        /// The minimal valid buffer size to be sent or be received.
        /// </summary>
        /// <seealso cref="Shared.CommandType"/>
        public const int MINIMAL_BUFFER_SIZE =
                2    // header
                + 2  // nickname + NUL
                + 2  // data + NUL
            ;

        /// <summary>
        /// The maximal valid buffer size to be sent or be received.
        /// </summary>
        /// <seealso cref="Shared.CommandType"/>
        public const int FINAL_BUFFER_MAX_SIZE =
            MINIMAL_BUFFER_SIZE + MAX_NICKNAME_SIZE + MAX_DATA_SIZE;

        /// <summary>
        /// The command to be sent or received (determined by <see cref="CommandType"/>).
        /// </summary>
        public readonly Command Command;

        /// <summary>
        /// The command type, whether the received command
        /// is a request or a response.
        /// </summary>
        /// <seealso cref="Command"/>
        public readonly CommandType CommandType;

        /// <summary>
        /// The data buffer to be sent or received.
        /// </summary>
        /// <seealso cref="CommandType"/>
        public readonly string Data;

        /// <summary>
        /// The nickname of the sender.
        /// </summary>
        public readonly string Nickname;

        /// <summary>
        /// Constructor of <see cref="ChatMessage"/>.
        /// </summary>
        /// <param name="command">See <see cref="Command"/>.</param>
        /// <param name="type">See <see cref="CommandType"/>.</param>
        /// <param name="nickname">See <see cref="Nickname"/>.</param>
        /// <param name="data">See <see cref="Data"/>.</param>
        public ChatMessage(Command command, CommandType type, string nickname, string data) {
            this.Command = command;
            this.CommandType = type;
            this.Nickname = nickname;
            this.Data = data;
        }

        /// <summary>
        /// Converts a received data buffer to a ChatMessage object.
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <exception cref="SyntaxErrorException"></exception>
        public ChatMessage(byte[] dataBuffer) {
            ushort cursorPosition = 0;

            if (dataBuffer.Length < MINIMAL_BUFFER_SIZE
                    || dataBuffer.Length > FINAL_BUFFER_MAX_SIZE) {
                throw new InvalidBufferLength(buffer: dataBuffer);
            }

            // parse the message header: the command and the command type
            this.Command = (Command)dataBuffer[cursorPosition++];
            this.CommandType = (CommandType)dataBuffer[cursorPosition++];

            // retrieve the nickname
            cursorPosition = ByteUtils.GetNulTerminatedString(
                dataBuffer, cursorPosition, MAX_NICKNAME_SIZE, out this.Nickname);

            // retrieve the data
            ByteUtils.GetNulTerminatedString(
                dataBuffer, ++cursorPosition, MAX_DATA_SIZE, out this.Data);
        }

        /// <summary>
        /// Converts a <see cref="ChatMessage"/> object to bytes.
        /// </summary>
        /// <returns>The <see cref="ChatMessage"/> data in bytes.</returns>
        public byte[] GetBytes() {
            var arraySize =
                2                            // header size: Command + CommandType
                + this.Nickname.Length + 1   // nickname + NUL
                + this.Data.Length + 1;      // data + NUL

            // Create the buffer and a cursor
            var buffer = new byte[arraySize];
            var cursorPosition = 0;

            // Add the header data
            buffer[cursorPosition++] = (byte)this.Command;
            buffer[cursorPosition++] = (byte)this.CommandType;

            // Add the nickname + NUL termination
            cursorPosition += Encoding.ASCII.GetBytes(
                this.Nickname, 0, this.Nickname.Length, buffer, cursorPosition);
            buffer[cursorPosition++] = byte.MinValue;

            // Add the data + NUL termination
            cursorPosition += Encoding.ASCII.GetBytes(
                this.Data, 0, this.Data.Length, buffer, cursorPosition);
            buffer[cursorPosition] = byte.MinValue;

            return buffer;
        }

        /// <summary>
        /// Represent a request or response to a string.
        /// </summary>
        /// <returns>The chat message's string representation.</returns>
        public override string ToString() {
            return
                "[" + this.CommandType.ToString("g") + "]" +   // The command type: IN or OUT
                "[" + this.Command.ToString("g") + "]" +       // The command
                "[" + this.Data.Length + "] " +                // The received data size
                this.Nickname + ": " + this.Data;              // "Nickname: data message"
        }
    }
}
