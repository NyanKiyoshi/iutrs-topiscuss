using Terminal.Gui;

namespace Client.views {
    public class InputBox : DialogWithError {
        private const int PADDING = 2;
        private TextField textField;

        public string TextInput => this.textField.Text.ToString();
        public delegate void OnSuccess(InputBox inputBox, string text);

        public InputBox(
                string title,
                int width, int height,
                string labelText,
                int minLength, int maxLength, OnSuccess onSuccess) : base(title, width, height) {

            var labelField = new Label(PADDING, 2, labelText);
            var textFieldXPosition = labelText.Length + PADDING * 2;

            // Create the input text field
            this.textField = new TextField(
                textFieldXPosition, 2, width - textFieldXPosition - PADDING * 3, "");

            // The the inline form (label: textInput)
            this.Add(labelField, this.textField);

            // Add actions buttons: Save and Cancel
            this.AddButton(new Button(3, 14, "Save") {
                Clicked = () => {
                    if (this.TextInput.Length < minLength) {
                        this.SetTitleError("Too Short!");
                    }
                    else if (this.TextInput.Length > maxLength) {
                        this.SetTitleError("Too Long!");
                    }
                    else {
                        this.RemoveError();
                        onSuccess(this, this.TextInput);
                        this.Running = false;
                    }
                }
            });

            // Show the dialog box
            Application.Run(this);
        }

        public InputBox(string title, string labelText, int minLength, int maxLength, OnSuccess onSuccess)
            : this(title, 60, 10, labelText, minLength, maxLength, onSuccess) {
        }
    }
}
