using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TronClient {
    public class Client {
        private Tron.Tron myTron;  // Moteur du jeu

        public byte frequence;  // Temps du tour de jeu (en dixieme de s)

        // constructeur : IP/Port du serveur
        public Client(String myServerIP, int myServerPort) {
            // TODO : Creation de la socket d'écoute TCP
        }

        // Appelé au début de la partie
        public Tron.Tron Init() {
            System.Console.WriteLine("Init");

            // TODO Connexion au serveur

            // TODO Réception des paramètres

            // TODO Initialisation de la fréquence : frequence = <frequence>
            frequence = 1;

            // TODO Initialisation du moteur :
            // myTron = new Tron(byte <taille terrain>, byte <nombre de joueurs>, byte <numéro du joueur>);
            myTron = new Tron.Tron(60, 2, 0);

            // Retourne le moteur
            return myTron;
        }

        // Appelé régulièrement à chaque tour de jeu
        public void Routine() {
            System.Console.WriteLine("Routine");

            // TODO Envoie de sa direction : myTron.getDirection()

            // TOSO Reception de toutes les directions : myTron.setDirections(byte[] < toutes les directions>);
        }

        // Appelé à la fin de la partie
        public void Conclusion() {
            System.Console.WriteLine("Conclusion");

            // fermeture socket
        }

        // propriété frequence (Ne pas toucher)
        public int freq {
            get { return frequence * 100; }
            set { }
        }
    }
}
