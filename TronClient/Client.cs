using System;
using System.Net;
using System.Net.Sockets;

namespace TronClient {
    public class Client {
        private TronLib.Tron myTron;  // Moteur du jeu
        private readonly Socket _clientSocket;
        private readonly IPEndPoint _serverEP;
        public byte frequence;        // Temps du tour de jeu (en dixieme de s)

        // constructeur : IP/Port du serveur
        public Client(string myServerIP, int myServerPort) {
            this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._clientSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            this._serverEP = new IPEndPoint(IPAddress.Parse(myServerIP), myServerPort);
        }

        // Appelé au début de la partie
        public TronLib.Tron Init() {
            Console.WriteLine("Init");

            // Connexion au serveur
            this._clientSocket.Connect(this._serverEP);

            // Réception des paramètres
            //bufferReception { taille, nombre de joueurs, numero du joueur, PERIODE }
            byte[] receiveBuffer = new byte[4];
            this._clientSocket.Receive(receiveBuffer, receiveBuffer.Length, SocketFlags.None);

            // Initialisation de la fréquence : frequence = <frequence>
            this.frequence = receiveBuffer[3];

            // Initialisation du moteur : myTron = new Tron(byte <taille terrain>, byte <nombre de joueurs>, byte <numéro du joueur>);
            this.myTron = new TronLib.Tron(receiveBuffer[0], receiveBuffer[1], receiveBuffer[2]);

            // Retourne le moteur
            return this.myTron;
        }

        // Appelé régulièrement à chaque tour de jeu
        public void Routine() {
            Console.WriteLine("Routine");

            // Envoie de sa direction : myTron.getDirection()
            this._clientSocket.Send(new byte[1] { this.myTron.GetDirection() });

            // Reception des directions des joueurs : myTron.setDirections(byte[] < toutes les directions>);
            byte[] playersDirectionsBuffer = new byte[this.myTron.GetNJoueurs()];
            this._clientSocket.Receive(playersDirectionsBuffer);

            // Mise à jour des positions des joueurs
            this.myTron.SetDirections(playersDirectionsBuffer);
        }

        // Appelé à la fin de la partie
        public void Conclusion() {
            Console.WriteLine("Conclusion");

            // fermeture socket
            this._clientSocket.Close();
        }


        // propriété frequence (Ne pas toucher)
        public int Freq => this.frequence * 100;
    }
}

