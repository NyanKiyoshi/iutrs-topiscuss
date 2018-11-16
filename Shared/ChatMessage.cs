namespace Shared {
    public enum Command {
        POST,
        GET,
        HELP,
        QUIT,
        STOPSERVEUR,
        SUBSCRIBE,
        SUBSCRIBE_V2,
        UNSUBSCRIBE
    }

    public enum CommandType {REQUEST, RESPONSE}

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

        public ChatMessage(Command command, CommandType type, string data, string nickname) {
            this.Command = command;
            this.CommandType = type;
            this.Data = data;
            this.Nickname = nickname;
        }

        /// <summary>
        /// Converts a received data buffer to a ChatMessage object.
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <exception cref="NULTerminationNotFound"></exception>
        public ChatMessage(byte[] dataBuffer) {
            ushort cursorPosition = 0;

            // parse the message header: the command and the command type
            this.Command = (Command)dataBuffer[cursorPosition++];
            this.CommandType = (CommandType)dataBuffer[cursorPosition++];

            // retrieve the nickname
            cursorPosition = ByteUtils.GetNulTerminatedString(
                dataBuffer, cursorPosition, MAX_NICKNAME_SIZE, out this.Nickname);

            // retrieve the data
            ByteUtils.GetNulTerminatedString(
                dataBuffer, cursorPosition, MAX_DATA_SIZE, out this.Data);
        }

        /// <summary>
        /// Converts a <see cref="ChatMessage"/> object to bytes.
        /// </summary>
        /// <returns>The <see cref="ChatMessage"/> data in bytes.</returns>
        public byte[] GetBytes() {
            return new byte[] {};
        }

        /// <summary>
        /// Represent a request or response to a string.
        /// </summary>
        /// <returns>The chat message's string representation.</returns>
        public override string ToString() {
            return
                "[" + this.CommandType.ToString("g") + "]" +   // The command type: IN or OUT
                "[" + this.Nickname + "]" +                    // The sender's nickname
                "[" + this.Command.ToString("g") + "]" +       // The command
                "[" + this.Data.Length + "] " +                // The received data size
                this.Data;                                     // The message
        }
    }
}
