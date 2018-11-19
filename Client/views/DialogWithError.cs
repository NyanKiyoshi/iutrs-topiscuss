using NStack;
using Terminal.Gui;

namespace Client.views {
    public class DialogWithError : Dialog {
        private readonly ustring _originalTitle;

        public DialogWithError(
            ustring title, int width, int height, params Button[] buttons)
                : base(title, width, height, buttons) {
            this._originalTitle = title;
        }

        public void SetTitleError(ustring shortMessage) {
            this.Title = string.Format("{0} ({1})", this._originalTitle, shortMessage);
            this.ColorScheme = Colors.Error;
        }

        public void RemoveError() {
            this.Title = this._originalTitle;
            this.ColorScheme = Colors.Dialog;
        }
    }
}
