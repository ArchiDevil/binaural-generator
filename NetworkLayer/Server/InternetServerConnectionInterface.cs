using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLayer
{
    public class InternetServerConnectionInterface : IServerConnectionInterface
    {
        string address = string.Empty;
        int port = -1;
        Socket listenerSocket = null;

        public bool Start(int port)
        {
            Stop();
            this.port = port;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList.Where((a, i) => a.AddressFamily == AddressFamily.InterNetwork).First();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(1);

                while (true)
                {
                    Socket handler = listenerSocket.Accept();
                    string data = null;
                    byte[] bytes = null;

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                            break;
                    }

                    byte[] msg = Encoding.ASCII.GetBytes(data);
                    handler.Send(msg);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
                return false;
            }

            return true;
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public int Receive(byte[] data)
        {
            throw new NotImplementedException();
        }

        public int Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<int> AsyncReceive(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<int> AsyncSend(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
