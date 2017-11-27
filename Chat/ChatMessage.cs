using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientUdp
{
    public enum Commande { 
        POST, GET, HELP, QUIT, STOPSERVEUR, SUBSCRIBE, SUBSCRIBEv2, UNSUBSCRIBE
    };

    public enum CommandeType { REQUETE, REPONSE };

    class ChatMessage
    {
        public const int bufferSize = 1500;

        public Commande commande;               // commande
        public CommandeType commandeType;       // type (Requête/Réponse)
        public int dataSize;                    // taille de la donnée
        public String data;                     // données de la commande

        public ChatMessage(Commande commande, CommandeType type, String data)
        {
            this.commande = commande;
            this.commandeType = type;
            this.dataSize = data.Length;
            this.data = data;
        }

        public ChatMessage(byte[] buffer)
        {
        }

        public byte[] GetBytes()
        {
        }

        public override string ToString()
        {
            return "[" + commande + "," + commandeType + ",\"" + pseudo + "\"," + dataSize + ",\"" + data + "\"]";
        }

    }
}
