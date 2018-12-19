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

            var myFormLobby = new FormLobby();
            Application.Run(myFormLobby);
        }
    }
}
