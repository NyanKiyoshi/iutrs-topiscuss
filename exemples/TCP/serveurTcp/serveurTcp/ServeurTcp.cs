using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServeurTcp
{
    class ServeurTcp
    {
        static void Main(string[] args)
        {
            try
            {
                //************************************************************** Initialisation

                // Creation de la socket d'écoute TCP
                Socket listenSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                // On lie la socket au point de communication
                listenSocket.Bind(new IPEndPoint(IPAddress.Any, 11111));

                // On la positionne en mode "ecoute" pour 10 clients en attente maximum
                listenSocket.Listen(10);

                Console.WriteLine("Attente d'une nouvelle connexion...");

                // Acceptation d'un client et creation de la socket connectee 
                Socket connectedSocket = listenSocket.Accept();

                Console.WriteLine("Nouveau client connecte : " + connectedSocket.RemoteEndPoint);

                //************************************************************** Communications
                // Reception message client
                byte[] buffer = new byte[80];
                int nBytes = connectedSocket.Receive(buffer, buffer.Length, SocketFlags.None);

                // Decodage bu buffer de bytes/ASCII vers une String
                String msg = System.Text.Encoding.ASCII.GetString(buffer, 0, nBytes);

                Console.WriteLine("Nouveau message de "
                    + connectedSocket.RemoteEndPoint
                    + " (" + nBytes + " octets)"
                    + ": \"" + msg + "\"");

                //************************************************************** Conclusion
                // fermetures sockets
                Console.WriteLine("Fermeture Socket...");
                connectedSocket.Close();
                listenSocket.Close();
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
