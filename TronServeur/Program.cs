
namespace TronServeur {
    public class Program {
        public static void Main(string[] args) {
            TronLib.Tron myTron;            // Moteur du jeu

            byte nJoueurs = 2;      // Nombre de joueurs
            byte frequence = 10;    // Temps du tour de jeu (en dixieme de s)
            byte taille = 60;       // Taille du terrain

            // ************************************* Intitialisation partie
            System.Console.WriteLine("Initialisation");

            // TODO Creation de la socket d'écoute TCP

            // TODO Creation du tableau des sockets connectées

            // Creation du moteur de jeu
            myTron = new TronLib.Tron(nJoueurs, taille);

            // TODO Bind et listen

            // TODO Acceptation des clients

            // TODO Envoie des paramètres

            // ************************************* Routine à chaque tour
            System.Console.WriteLine("Routine");

            // Tant que la partie n'est pas finie
            while (!myTron.IsFinished()) {
                // TODO Réception de la direction de chaque joueur

                // TODO Calcul collision : myTron.Collision(byte[] <toutes les directions>);

                // TODO Envoie des directions de tous les joueurs à tous les clients
            }


            // ************************************* Conclusion
            System.Console.WriteLine("Conclusion");

            // TODO Fermeture des sockets connectées

            // TODO Fermeture socket d'écoute
        }
    }
}
