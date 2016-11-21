using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Timers;


namespace TronClient
{
    public partial class FormTron : Form
    {
        // Ces variables sont les données membres. Elles sont accessibles n'importe où dans la classe.
        private Graphics e;
        private Bitmap bmp;

        private Client myClient;
        private Tron.Tron myTron;

        private int tailleJoueur = 5;

        private KeyEventHandler keh;
        private System.Timers.Timer aTimer;
        
        private bool stillAlive;

        public FormTron(Client myMyClient)
        {
            this.Visible = false;

            myClient = myMyClient;
            myTron = myClient.Init();
            InitializeComponent();

            pictureBox1.Size = new System.Drawing.Size(myTron.getTaille() * tailleJoueur, myTron.getTaille() * tailleJoueur);
            this.ClientSize = new System.Drawing.Size(myTron.getTaille() * tailleJoueur, myTron.getTaille() * tailleJoueur);

            // Creation bitmap de dessin
            bmp = new System.Drawing.Bitmap(myTron.getTaille() * tailleJoueur, myTron.getTaille() * tailleJoueur);
            e = System.Drawing.Graphics.FromImage(bmp);
            this.ResumeLayout(false);

            keh = new KeyEventHandler(OnKeyPress);
            
            this.KeyDown += keh;

            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = myClient.freq;
            aTimer.Enabled = true;

            stillAlive = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs eventArgs)
        {
            aTimer.Enabled = false;

            myClient.Routine();
            myTron.Deplacement();


            if (myTron.IsDead() && stillAlive) {
                stillAlive = false;

                // Désactivation clavier
                this.KeyDown -= keh;
                //MessageBox.Show(null, "You LOSE!!!", "Conclusion",
                //     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                e.DrawString("You LOSE!!!", new Font(FontFamily.GenericSansSerif, 20.00f),
                    new SolidBrush(Color.Red), new PointF(myTron.getTaille() * tailleJoueur / 2 - 80 , myTron.getTaille() * tailleJoueur / 2-10));

            }
            if (myTron.IsFinished())
            {
                myClient.Conclusion();

                if (myTron.IsWinner())
                    //MessageBox.Show(null, "You WIN!!!", "Conclusion",
                    //     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    e.DrawString("You WIN!!!", new Font(FontFamily.GenericSansSerif, 20.00f),
                        new SolidBrush(Color.Blue), new PointF(myTron.getTaille() * tailleJoueur / 2 - 80, myTron.getTaille() * tailleJoueur / 2-10));
            }
            else aTimer.Enabled = true;

            Rendu();
        }
        
        public void Rendu()
        {
            for (int i = 0; i < myTron.getNJoueurs(); i++)
            {
                SolidBrush b;
                if (myTron.getMonNum() == i) b = new SolidBrush(Color.Blue);
                else b = new SolidBrush(Color.Red);

                e.FillRectangle(b,
                    new Rectangle(new Point(myTron.getPosX(i) * tailleJoueur, myTron.getPosY(i) * tailleJoueur),
                    new Size(tailleJoueur, tailleJoueur)));
            }

            // On affiche notre image Bitmap dans la PictureBox
            pictureBox1.Image = bmp;
        }

        public void OnKeyPress(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left  : myTron.GoLeft(); break;
                case Keys.Right : myTron.GoRight(); break;
                case Keys.Up    : myTron.GoUp(); break;
                case Keys.Down  : myTron.GoDown(); break;
            }

        }

    }

}


