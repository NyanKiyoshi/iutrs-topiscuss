using System;
using System.Windows.Forms;

namespace TronClient {
    // Form du lobby
    public partial class FormLobby : Form {
        private Client _myClient;

        public FormLobby() {
            this.InitializeComponent();
        }

        // Lance la partie
        private void StartTron() {
            // Création du client avec IP et Port
            this._myClient = new Client(textBoxIP.Text, int.Parse(textBoxPort.Text));

            // Création et affichage de la Form de jeu
            var myFormTron = new FormTron(_myClient);
            myFormTron.Show();
        }

        // Appelé au click sur le bouton start
        private void StartButton_Click(object sender, EventArgs e) {
            // Lance la partie
            this.StartTron();
        }
    }
}
