using System;
using System.Net;
using System.Net.Sockets;

namespace TronServeur {
    public class Program {
        private TronLib.Tron _myTron;
        private Socket _listenSocket;
        private Socket[] _listConnectedSockets;

        private readonly byte _nJoueurs;    // Nombre de joueurs
        private readonly byte _frequence;   // Temps du tour de jeu (en dixieme de s)
        private readonly byte _taille;      // Taille du terrain

        public Program(byte nJoueurs, byte frequence, byte tailleTerrain) {
            this._nJoueurs = nJoueurs;
            this._frequence = frequence;
            this._taille = tailleTerrain;
        }

        public void Start() {
            Console.WriteLine("Initialisation");
            Init();

            Console.WriteLine("Routine");
            Routine();

            Console.WriteLine("Conclusion");
            Conclusion();
        }

        private void WaitForPlayers() {
            Socket connectedSocket;

            // Acceptation des clients pour chaque client (joueur)
            for (byte numJoueur = 0; numJoueur < _nJoueurs; numJoueur++) {
                Console.WriteLine("Attente d'une nouvelle connexion...");
                connectedSocket = this._listenSocket.Accept();
                Console.WriteLine("Nouveau client connecté : {0}", connectedSocket.RemoteEndPoint);
                this._listConnectedSockets[numJoueur] = connectedSocket;
            }
        }

        private void SendParameters() {
            for (byte numJoueur = 0; numJoueur < this._nJoueurs; numJoueur++) {
                var bufferParameters = new byte[4] {
                    _taille, this._nJoueurs, numJoueur, this._frequence };
                var nBytesParameters = this._listConnectedSockets[numJoueur].Send(
                        bufferParameters, 0, bufferParameters.Length, SocketFlags.None);

                Console.WriteLine(
                    "Envoi des parametres vers le joueur {0} ({1} octets)",
                    this._listConnectedSockets[numJoueur].RemoteEndPoint, nBytesParameters);
            }
        }

        private void Init() {
            // Creation de la socket d'écoute TCP
            this._listenSocket =
                new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Creation du tableau des sockets connectées
            this._listConnectedSockets = new Socket[_nJoueurs];

            // Creation du moteur de jeu
            this._myTron = new TronLib.Tron(this._nJoueurs, this._taille);

            // Bind et listen
            this._listenSocket.Bind(new IPEndPoint(IPAddress.Any, 8000));
            this._listenSocket.Listen(_nJoueurs);

            // Attend que les joueurs soient là
            this.WaitForPlayers();

            // Envoi des paramètres pour chaque client (joueur)
            this.SendParameters();
        }

        private void RetrievePositions(byte[] listDirections) {
            for (byte numJoueur = 0; numJoueur < this._nJoueurs; numJoueur++) {
                var connectedSock = this._listConnectedSockets[numJoueur];
                if (connectedSock == null) continue;

                try {
                    // Réception de la direction de chaque joueur
                    var bufferDirections = new byte[1];
                    connectedSock.Receive(bufferDirections, bufferDirections.Length, SocketFlags.None);
                    listDirections[numJoueur] = bufferDirections[0];

                    Console.WriteLine(
                        "Reception de la direction du client {0} : {1}", connectedSock, listDirections[numJoueur]);
                }
                catch (SocketException) {
                    this._listConnectedSockets[numJoueur] = null;
                }
            }
        }

        private void SendPositions(byte[] listDirections) {
            for (byte numJoueur = 0; numJoueur < _nJoueurs; numJoueur++) {
                var connectedSock = this._listConnectedSockets[numJoueur];
                if (connectedSock == null) continue;

                try {
                    connectedSock.Send(
                        this._myTron.GetDirections(),
                        this._myTron.GetDirections().Length, SocketFlags.None);
                    Console.WriteLine(
                        "Envoi de la liste des directions au client {0}", this._listConnectedSockets[numJoueur]);
                }
                catch (SocketException) {
                    this._listConnectedSockets[numJoueur] = null;
                }
            }
        }

        private void Routine() {
            // liste de la commande des joueurs dans le tour courant 
            var listDirections = new byte[this._nJoueurs];

            // Tant que la partie n'est pas finie
            while (!this._myTron.IsFinished()) {
                this.RetrievePositions(listDirections);
                this._myTron.SetDirections(listDirections);

                // Calcul collision : myTron.Collision(byte[] <toutes les directions>);
                this._myTron.Collision(listDirections);

                // Envoie des directions de tous les joueurs à tous les clients
                this.SendPositions(listDirections);
            }
        }

        private void Conclusion() {
            // Fermeture des sockets connectées
            foreach (var connectedSocket in this._listConnectedSockets) {
                if (connectedSocket != null) {
                    connectedSocket.Close();
                }
            }

            // Fermeture socket d'écoute
            this._listenSocket.Close();
        }

        public static void Main(string[] args) {
            byte numJoueurs;

            do {
                Console.Write("Nombre de joueurs : ");
            } while (!byte.TryParse(Console.ReadLine(), out numJoueurs) || numJoueurs < 2);

            new Program(numJoueurs, 1, 60).Start();
        }
    }
}
