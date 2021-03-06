using System;

namespace TronLib {
    public class Tron {
        private readonly byte taille;    // taille du terrain
        private readonly byte nJoueurs;  // nombre de joueurs
        private readonly byte monNum;    // numéro du joueur courrant

        protected byte[] directions;     // 1=haut, 2=bas, 3=gauche, 4=droite, 5=perdu, 6=gagne
        protected int[,] marque;         // qui est passé où
        protected int[] posX, posY;      // positions courrante des joueurs

        protected byte oldDirection;     // ancienne position du joueur courrant
        protected int nVivants = 0;      // nombre de joueurs encore vivant

        // Constructeur à utiliser côté serveur
        public Tron(byte myNJoueurs, byte myTaille) : this(myTaille, myNJoueurs, 0) { }

        // Constructeur à utiliser côté client
        public Tron(byte myTaille, byte myNJoueurs, byte myMonNum) {
            taille = myTaille;
            nJoueurs = myNJoueurs;
            monNum = myMonNum;

            posX = new int[nJoueurs];
            posY = new int[nJoueurs];
            directions = new byte[nJoueurs];
            marque = new int[taille, taille];

            oldDirection = 0;

            Init();
        }

        public byte GetTaille() => taille;
        public byte GetNJoueurs() => nJoueurs;
        public byte GetMonNum() => monNum;
        public int GetPosX(int i) => posX[i];
        public int GetPosY(int i) => posY[i];

        public byte GetDirection() => directions[monNum];
        public byte[] GetDirections() => directions;
        public void SetDirections(byte[] d) => directions = d;

        // initialisation du terrain
        public void Init() {
            for (var i = 0; i < nJoueurs; i++) {
                directions[i] = (byte)(3 - (i % 2));
                posX[i] = taille / 4 * (1 + 2 * (i % 2));
                if (nJoueurs % 2 == 1) {
                    posY[i] = taille / (nJoueurs + 1) * (i + 1);
                }
                else {
                    posY[i] = taille / (nJoueurs / 2 + 1) * ((int)(i / 2) + 1);
                }

                Console.WriteLine(posX[i] + "," + posY[i]);
            }

            for (var i = 0; i < taille; i++) {
                for (var j = 0; j < taille; j++) {
                    marque[i, j] = -1;
                }
            }
        }

        // deplacement des joueurs
        // MAJ des PosX et posY en fonction de directions
        public void Deplacement() {
            for (var i = 0; i < nJoueurs; i++) {
                switch (directions[i]) {
                    case 0:
                        posY[i]--;
                        if (posY[i] < 0) posY[i] = taille - 1;
                        break;
                    case 1:
                        posY[i]++;
                        if (posY[i] >= taille) posY[i] = 0;
                        break;
                    case 2:
                        posX[i]--;
                        if (posX[i] < 0) posX[i] = taille - 1;
                        break;
                    case 3:
                        posX[i]++;
                        if (posX[i] >= taille) posX[i] = 0;
                        break;
                }
            }
        }

        // Detection des collisions
        // MAJ de directions en fonction des collisions
        // Appelle Deplacement()
        public void Collision() => Collision(directions);

        public void Collision(byte[] myDirections) {
            directions = myDirections;

            nVivants = 0;
            var vivant = -1;

            Deplacement();

            for (var i = 0; i < nJoueurs; i++) {
                if (marque[posX[i], posY[i]] != -1) {
                    directions[i] = 4;
                    Console.WriteLine("fail " + i);
                }

                else {
                    for (var j = i + 1; j < nJoueurs; j++) {
                        if (posX[i] == posX[j] && posY[i] == posY[j]) {
                            Console.WriteLine("fail " + i + " " + j);
                            directions[i] = 4;
                            directions[j] = 4;
                        }
                    }
                    marque[posX[i], posY[i]] = i;
                }
                if (directions[i] != 4) {
                    nVivants++;
                    vivant = i;
                }
            }

            if (nVivants == 1) {
                directions[vivant] = 5;
            }
        }

        // modifie la direction du joueur courrant
        public void GoUp() => directions[monNum] = 0;
        public void GoDown() => directions[monNum] = 1;
        public void GoLeft() => directions[monNum] = 2;
        public void GoRight() => directions[monNum] = 3;

        /// <returns>Vrai si le joueur courrant est mort</returns>
        public bool IsDead() => (directions[monNum] == 4);

        /// <returns>Vrai si le joueur courrant est gagnant</returns>
        public bool IsWinner() => (directions[monNum] == 5);

        //retourne vrai si la partie est finie
        public bool IsFinished() {
            nVivants = 0;
            for (var i = 0; i < nJoueurs; i++) {
                if (directions[i] < 4) {
                    nVivants++;
                }
            }

            return (nVivants < 2);
        }

    }
}
