using System;
using System.Text;
using System.Configuration;
using System.Xml;

using System.Net;
using System.Net.Sockets;

using Server_PMV.Models;

namespace Server_PMV
{
    public class Program
    {
        static string data = null;

        static void Main(string[] args)
        {
            Console.WriteLine("SERVER PER PANNELLI A MESSAGGIO VARIABILE AUTOSTRADALI");

            IPAddress serverIp = null;
            int serverPort;

            //Selezione dell IP da usare nella comunicazione
            do
            {
                int ipSel = -1;

                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                {
                    Console.WriteLine($"{i}) {ipHostInfo.AddressList[i]}");
                }

                if(int.TryParse(Console.ReadLine(), out ipSel) && ipSel < ipHostInfo.AddressList.Length)
                {
                    serverIp = ipHostInfo.AddressList[ipSel];
                    break;
                }
            } while (true);

            if(!int.TryParse(ConfigurationManager.AppSettings["PortUsed"], out serverPort))
            {
                Console.WriteLine("Porta impostata nei settaggi errata");
                return;
            }


            byte[] bytes = new byte[1024];
  
            IPEndPoint localEndPoint = new IPEndPoint(serverIp, serverPort);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(serverIp.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            ActionType actionType;
            XmlNode actionXmlData;
            string clientResponse;

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine($"[{DateTime.UtcNow.ToString("T")}] -- Aspettando una connessione sulla porta {serverPort}...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.  
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    // Show the data on the console.  
                    Console.WriteLine($"[{DateTime.UtcNow.ToString("T")}] -- {data}\n");

                    //Eseuo l'azione richiesta dal client parsando l'XML ricevuto
                    actionType = Action.Parse(data, out actionXmlData);
                    switch (actionType)
                    {
                        case ActionType.NotAction:
                            clientResponse = Action.Send("<?xml version=\"1.0\" encoding=\"utf-8\" ?><action><type>NotAction</type></action>");
                            break;
                        case ActionType.TestConn:
                            clientResponse = Action.Send("<?xml version=\"1.0\" encoding=\"utf-8\" ?><action><type>TestConn</type></action>");
                            break;
                        case ActionType.GetMessages:
                            clientResponse = Action.Send(Action.GetMessages());
                            break;
                        case ActionType.GetMessagesToView:
                            clientResponse = Action.Send(Action.GetMessages(true));
                            break;
                        case ActionType.AddMessage:
                            clientResponse = Action.Send(Action.AddMessage(actionXmlData));
                            break;
                        case ActionType.EditMessage:
                            clientResponse = Action.Send(Action.EditMessage(actionXmlData));
                            break;
                        case ActionType.MakeMessageToView:
                            clientResponse = Action.Send(Action.MakeMessageToView(actionXmlData));
                            break;
                        case ActionType.DeleteMessage:
                            clientResponse = Action.Send(Action.DeleteMessage(actionXmlData));
                            break;
                        default:
                            clientResponse = Action.Send("<?xml version=\"1.0\" encoding=\"utf-8\" ?><type>NotAction</type></action>");
                            break;
                    }

                    // Echo the data back to the client.  
                    byte[] msg = Encoding.UTF8.GetBytes(clientResponse);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    Console.WriteLine($"[{DateTime.UtcNow.ToString("T")}] -- Risposta Client -- {clientResponse}\n");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPremi qualunque tasto per terminare ...");
            Console.ReadKey();

        }
    }
}
