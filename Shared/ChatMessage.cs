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
        /// The maximal size of the buffer to be sent or be received.
        /// </summary>
        /// <seealso cref="CommandType"/>
        public const int BUFFER_SIZE = 1500;

        /// <summary>
        /// The command to be sent or received (determined by <see cref="_commandType"/>).
        /// </summary>
        private readonly Command _command;

        /// <summary>
        /// The command type, whether the received command
        /// is a request or a response.
        /// </summary>
        /// <seealso cref="_command"/>
        private readonly CommandType _commandType;

        /// <summary>
        /// The data buffer size to be sent or received.
        /// </summary>
        /// <seealso cref="_commandType"/>
        private readonly int _dataSize;

        /// <summary>
        /// The data buffer to be sent or received.
        /// </summary>
        /// <seealso cref="_commandType"/>
        private readonly string _data;

        /// <summary>
        /// The nickname of the sender.
        /// </summary>
        private readonly string _nickname;

        public ChatMessage(Command command, CommandType type, string data, string nickname) {
            this._command = command;
            this._commandType = type;
            this._dataSize = data.Length;
            this._data = data;
            this._nickname = nickname;
        }

        /// <summary>
        /// Converts a received data buffer to a ChatMessage object.
        /// </summary>
        /// <param name="dataBuffer"></param>
        public ChatMessage(byte[] dataBuffer) {
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
                "[" + this._commandType.ToString("g") + "]" +   // The command type: IN or OUT
                "[" + this._nickname + "]" +                    // The sender's nickname
                "[" + this._command.ToString("g") + "]" +       // The command
                "[" + this._dataSize + "] " +                   // The received data size
                this._data;                                     // The message
        }
    }
}
