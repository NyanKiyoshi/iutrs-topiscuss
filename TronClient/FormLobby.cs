using System;
using System.Net.Sockets;
using System.Windows.Forms;
using Client;

namespace TronClient {
    // Form du lobby
    public partial class FormLobby : Form {
        private Client _myClient;
        private DisposableClient _disposableChatClient;
        private string ChatText = string.Empty;

        public FormLobby() {
            this.InitializeComponent();
            this.commandBox.Items.AddRange(Enum.GetNames(typeof(Shared.Command)));
            this.commandBox.SelectedItem = Shared.Command.POST;
            this.inputBox.KeyUp += OnKeyUpChatBox;

            this._disposableChatClient = new DisposableClient(
                new System.Net.IPEndPoint(
                    Shared.DefaultConfig.DEFAULT_SERVER_HOST,
                    Shared.DefaultConfig.DEFAULT_SERVER_PORT
                )
            );
            this._disposableChatClient.MessageReceivedEvent += (receivedMessage, remoteEndpoint) => {
                this.ChatText += Environment.NewLine + receivedMessage;
                this.Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            this.richTextBox1.Text = this.ChatText;
        }

        protected override void OnLoad(EventArgs e) {
            this.ActiveControl = this.inputBox;
            this.inputBox.Focus();
            this._disposableChatClient.SendMessage(new Shared.ChatMessage(
                Shared.Command.SUB, Shared.CommandType.REQUEST, "Client", ""));
        }

        protected void OnKeyUpChatBox(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Return && this.commandBox.SelectedItem != null) {
                var selectedCommand = (Shared.Command)this.commandBox.SelectedItem;
                e.Handled = true;
            }
        }

        // Lance la partie
        private void StartTron() {
            // Création du client avec IP et Port
            this._myClient = new Client(textBoxIP.Text, int.Parse(textBoxPort.Text));

            // Création et affichage de la Form de jeu
            try {
                var myFormTron = new FormTron(_myClient);
                myFormTron.Show();
            }
            catch (SocketException exc) {
                MessageBox.Show(
                    this,
                    "Erreur de connexion ! " + exc.Message,
                    "Erreur de connexion",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Appelé au click sur le bouton start
        private void StartButton_Click(object sender, EventArgs e) {
            // Lance la partie
            this.StartTron();
        }
    }
}
