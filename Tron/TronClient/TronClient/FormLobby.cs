using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TronClient
{
    // Form du lobby
    public partial class FormLobby : Form
    {
        Client myClient;

        public FormLobby()
        {
            InitializeComponent();
        }

        // Lance la partie
        private void StartTron()
        {
            // Création du client avec IP et Port
            myClient = new Client(textBoxIP.Text, int.Parse(textBoxPort.Text));

            // Création et affichage de la Form de jeu
            FormTron myFormTron = new FormTron(myClient);
            myFormTron.Show();
        }

        // Appelé au click sur le bouton start
        private void button1_Click(object sender, EventArgs e)
        {
            // Lance la partie
            StartTron();
        }
    }
}
