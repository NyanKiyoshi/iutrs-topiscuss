using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServeurUdp
{
    class ServeurUdp
    {
        static void Main(string[] args)
        {

            try
            {
                // ************************************************************** Initialisation

                // Création de la socket d'écoute UDP
                Socket serverSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Dgram,
                    ProtocolType.Udp);


                // Liaison de la socket au point de communication
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, 11111));

                
                //************************************************************** Communications
                Console.WriteLine("Attente d'une nouveau message...");

                // Reception message client
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = new byte[80];
                int nBytes = serverSocket.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref clientEP);

                // Decodage du buffer de bytes en ASCII vers un string
                String msg = System.Text.Encoding.ASCII.GetString(buffer, 0, nBytes);

                Console.WriteLine("Nouveau message de "
                    + clientEP
                    + " (" + nBytes + " octets)"
                    + ": \"" + msg + "\"");


                //************************************************************** Conclusion
                // Fermeture socket
                Console.WriteLine("Fermeture Socket...");
                serverSocket.Close();
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
