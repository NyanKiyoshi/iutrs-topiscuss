using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientTcp
{
    class ClientTcp
    {
        static void Main(string[] args)
        {
            try
            {
                //************************************************************** Initialisation
                string serverIP = "0.0.0.0";
                int serverPort = 00000;


                // Creation de la socket d'écoute TCP
                Socket clientSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);


                Console.WriteLine("Tentative de connexion...");

                // Liaison de la socket au point de communication
                clientSocket.Bind(new IPEndPoint(IPAddress.Any, 22222));

                // Creation du EndPoint serveur
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

                // Connexion au serveur
                clientSocket.Connect(serverEP);

                Console.WriteLine("Client connecté...");

                // Lecture message au clavier
                Console.Write("? ");
                String msg = Console.ReadLine();


                //************************************************************** Communications
                // Encodage de la String en buffer de bytes/ASCII 
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(msg);

                // Envoie du message au serveur
                int nBytes = clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);

                Console.WriteLine("Nouveau message envoye vers "
                    + clientSocket.RemoteEndPoint
                    + " (" + nBytes + " octets)"
                    + ": \"" + msg + "\"");

                //************************************************************** Conclusion
                // Fermeture socket
                Console.WriteLine("Fermeture Socket...");
                clientSocket.Close();

            }
            catch (SocketException E)
            {
                Console.WriteLine(E.Message);
                Console.ReadKey();
            }

            Console.ReadKey();
        }
    }
}
