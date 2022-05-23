using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client_Terminal_PMV
{
    public class Connection
    {
        internal static string SendReceiveFromServer(IPAddress IpServer, string strToSend)
        {
            byte[] bytes = new byte[2048]; //500KB 0.5MB
            string incomingResponseServer = String.Empty;

            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IpServer, 11000);
                Socket sender = new Socket(IpServer.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.SendTimeout = 1000;
                sender.ReceiveTimeout = 2500;
                try
                {
                    sender.Connect(remoteEP);

                    //Console.WriteLine("Socket connected to {0}",
                    //    sender.RemoteEndPoint.ToString());

                    if (strToSend.IndexOf("<EOF>") < 0)
                        strToSend += "<EOF>";

                    byte[] msg = Encoding.UTF8.GetBytes(strToSend);
                    int bytesSent = sender.Send(msg);

                    while (true)
                    {
                        int bytesRec = sender.Receive(bytes);
                        incomingResponseServer += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (incomingResponseServer.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    bytes = null;
                    return incomingResponseServer;

                }
                catch (Exception)
                {
                    bytes = null;
                    return null;
                }

            }
            catch (Exception)
            {
                bytes = null;
                return null;
            }
        }

        public static bool Ping(IPAddress iPAddressToVerify)
        {
            Ping verifyIP = new Ping();
            int timeout = 120;
            PingReply reply = verifyIP.Send(iPAddressToVerify, timeout);
            //PingReply reply = verifyIP.Send(IPAddress.Parse("1.1.1.1"), timeout); //Attenzione, questa stringa limita l'uso dei client a quelli connessi a internet

            if (reply.Status == IPStatus.Success)
                return true;
            else
                return false;
        }

        public static IPAddress GetWorkingIpAddress(IPAddress serverToPing)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress iPAddress in ipHostInfo.AddressList)
            {
                if (iPAddress.ToString().Count(c => c == '.') == 3)
                {
                    if (Ping(serverToPing))
                        return iPAddress;
                    else
                        continue;
                }
                else
                    continue;
            }
            return null;
        }

        public static bool TestConnToServer(IPAddress serverIp)
        {
            string received = Connection.SendReceiveFromServer(serverIp, "<?xml version=\"1.0\" encoding=\"utf - 8\"?><action><type>TestConn</type></action>");
            return Action.Parse(received) == ActionType.TestConn;
        }
    }
}
