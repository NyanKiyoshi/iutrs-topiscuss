using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientUdp {
    class ClientUdp {
        static void Main(string[] args) {
            try {
                //************************************************************** Initialisation
                string serverIP = "0.0.0.0"; // A changer
                int serverPort = 000000; // A changer


                // Création de la socket d'écoute UDP
                Socket clientSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp);


                // Liaison de la socket au point de communication
                clientSocket.Bind(new IPEndPoint(IPAddress.Any, 22222));


                // Création du EndPoint serveur
                EndPoint serverEP = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);


                // Lecture message au clavier
                Console.Write("? ");
                String msg = Console.ReadLine();


                //************************************************************** Communications

                // Encodage du string dans un buffer de bytes en ASCII
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(msg);
                Console.WriteLine("Taille buffer : " + buffer.Length);

                // Envoie du message au serveur
                int nBytes = clientSocket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, serverEP);

                Console.WriteLine("Nouveau message envoye vers "
                                  + serverEP
                                  + " (" + nBytes + " octets)"
                                  + ": \"" + msg + "\"");


                //************************************************************** Conclusion
                // Fermeture socket
                Console.WriteLine("Fermeture Socket...");
                clientSocket.Close();


            }
            catch (SocketException E) {
                Console.WriteLine(E.Message);
                Console.ReadKey();
            }

            Console.ReadKey();
        }
    }
}
