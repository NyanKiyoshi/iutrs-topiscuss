using System.Drawing;
using System.Windows.Forms;
using System.Timers;

namespace TronClient {
    public partial class FormTron : Form {
        // Ces variables sont les données membres. Elles sont accessibles n'importe où dans la classe.
        private Graphics _e;
        private Bitmap _bmp;

        private Client _myClient;
        private TronLib.Tron _myTron;

        private int tailleJoueur = 5;

        private KeyEventHandler _keh;
        private System.Timers.Timer _aTimer;

        private bool _stillAlive;

        public FormTron(Client myMyClient) {
            this.Visible = false;

            this._myClient = myMyClient;
            this._myTron = _myClient.Init();
            this.InitializeComponent();

            this.pictureBox1.Size = new Size(
                this._myTron.GetTaille() * this.tailleJoueur,
                this._myTron.GetTaille() * this.tailleJoueur);
            this.ClientSize = new Size(
                this._myTron.GetTaille() * this.tailleJoueur,
                this._myTron.GetTaille() * this.tailleJoueur);

            // Creation bitmap de dessin
            this._bmp = new Bitmap(
                this._myTron.GetTaille() * this.tailleJoueur,
                this._myTron.GetTaille() * this.tailleJoueur);
            this._e = Graphics.FromImage(this._bmp);
            this.ResumeLayout(false);

            this._keh = new KeyEventHandler(OnKeyPress);
            this.KeyDown += this._keh;

            this._aTimer = new System.Timers.Timer();
            this._aTimer.Elapsed += new ElapsedEventHandler(this.OnTimedEvent);
            this._aTimer.Interval = this._myClient.Freq;
            this._aTimer.Enabled = true;

            this._stillAlive = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs eventArgs) {
            this._aTimer.Enabled = false;

            this._myClient.Routine();
            this._myTron.Deplacement();


            if (this._myTron.IsDead() && this._stillAlive) {
                this._stillAlive = false;

                // Désactivation clavier
                this.KeyDown -= _keh;
                //MessageBox.Show(null, "You LOSE!!!", "Conclusion",
                //     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                this._e.DrawString(
                    "You LOSE!!!",
                    new Font(FontFamily.GenericSansSerif, 20.00f),
                    new SolidBrush(Color.Red),
                    new PointF(
                        this._myTron.GetTaille() * this.tailleJoueur / 2 - 80,
                        this._myTron.GetTaille() * this.tailleJoueur / 2 - 10
                    )
                );

            }
            if (this._myTron.IsFinished()) {
                this._myClient.Conclusion();

                if (this._myTron.IsWinner()) {
                    //MessageBox.Show(null, "You WIN!!!", "Conclusion",
                    //     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    this._e.DrawString(
                        "You WIN!!!",
                        new Font(FontFamily.GenericSansSerif, 20.00f),
                        new SolidBrush(Color.Blue),
                        new PointF(
                            this._myTron.GetTaille() * this.tailleJoueur / 2 - 80,
                            this._myTron.GetTaille() * this.tailleJoueur / 2 - 10
                        )
                    );
                }
            }
            else {
                this._aTimer.Enabled = true;
            }

            this.Rendu();
        }

        public void Rendu() {
            for (var i = 0; i < this._myTron.GetNJoueurs(); i++) {
                SolidBrush b;
                if (this._myTron.GetMonNum() == i) {
                    b = new SolidBrush(Color.Blue);
                }
                else {
                    b = new SolidBrush(Color.Red);
                }

                this._e.FillRectangle(
                    b,
                    new Rectangle(
                        new Point(
                            this._myTron.GetPosX(i) * this.tailleJoueur,
                            this._myTron.GetPosY(i) * this.tailleJoueur
                        ),
                        new Size(this.tailleJoueur, this.tailleJoueur)
                    )
                );
            }

            // On affiche notre image Bitmap dans la PictureBox
            this.pictureBox1.Image = _bmp;
        }

        public void OnKeyPress(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Left: this._myTron.GoLeft(); break;
                case Keys.Right: this._myTron.GoRight(); break;
                case Keys.Up: this._myTron.GoUp(); break;
                case Keys.Down: this._myTron.GoDown(); break;
            }
        }
    }
}
