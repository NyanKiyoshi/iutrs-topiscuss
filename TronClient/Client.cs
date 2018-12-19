using System;

namespace TronClient {
    public class Client {
        private TronLib.Tron myTron;  // Moteur du jeu
        public byte frequence;        // Temps du tour de jeu (en dixieme de s)

        // constructeur : IP/Port du serveur
        public Client(string myServerIP, int myServerPort) {
            // TODO : Creation de la socket d'écoute TCP
        }

        // Appelé au début de la partie
        public TronLib.Tron Init() {
            Console.WriteLine("Init");

            // TODO Connexion au serveur       

            // TODO Réception des paramètres

            // TODO Initialisation de la fréquence : frequence = <frequence>
            this.frequence = 1;

            // TODO Initialisation du moteur : myTron = new Tron(byte <taille terrain>, byte <nombre de joueurs>, byte <numéro du joueur>);
            this.myTron = new TronLib.Tron(60, 2, 0);

            // Retourne le moteur
            return this.myTron;
        }

        // Appelé régulièrement à chaque tour de jeu
        public void Routine() {
            Console.WriteLine("Routine");

            // TODO Envoie de sa direction : myTron.getDirection()

            // TOSO Reception de toutes les directions : myTron.setDirections(byte[] < toutes les directions>);
        }

        // Appelé à la fin de la partie
        public void Conclusion() {
            Console.WriteLine("Conclusion");

            // fermeture socket
        }


        // propriété frequence (Ne pas toucher)
        public int Freq => this.frequence * 100;
    }
}

