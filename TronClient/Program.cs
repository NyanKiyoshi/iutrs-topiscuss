using System;
using System.Windows.Forms;

namespace TronClient {
    public static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var promptForm = new InputBox(
                "What opacity? (byte)", DEFAULT_OPACITY.ToString());

            if (promptForm.ShowDialog() != DialogResult.OK) {
                Environment.Exit(0);
            }

            var myFormLobby = new FormLobby(promptForm.InputText);
            Application.Run(myFormLobby);
        }
    }
}
