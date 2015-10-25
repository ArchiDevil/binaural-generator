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
    public class InternetClientConnectionInterface : IClientConnectionInterface
    {
        string address = string.Empty;
        int port = -1;
        Socket sender = null;

        public bool Connect(string address, int port)
        {
            Disconnect();

            this.address = address;
            this.port = port;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(address);
            IPAddress ipAddress = ipHostInfo.AddressList.Where((a, i) => a.AddressFamily == AddressFamily.InterNetwork).First();

            if (ipAddress == null)
                return false;

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sender.Connect(remoteEP);

                byte[] msg = Encoding.ASCII.GetBytes("<EOF>");
                sender.Send(msg);

                // waiting for the response...
                msg = new byte[1024];
                int count = sender.Receive(msg);

                string response = Encoding.ASCII.GetString(msg, 0, count);
                if (response.IndexOf("<EOF>") == -1)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
                return false;
            }

            return true;
        }

        public void Disconnect()
        {
            if (sender != null)
            {
                sender.Disconnect(false);
                sender.Close();
                sender = null;
            }
        }

        public int Receive(byte[] data)
        {
            if (sender == null)
                return 0;

            throw new NotImplementedException();
        }

        public int Send(byte[] data)
        {
            if (sender == null)
                return 0;

            throw new NotImplementedException();
        }

        public Task<int> AsyncReceive(byte[] data)
        {
            if (sender == null)
                return Task.FromResult(0);

            throw new NotImplementedException();
        }

        public Task<int> AsyncSend(byte[] data)
        {
            if (sender == null)
                return Task.FromResult(0);

            throw new NotImplementedException();
        }
    }
}
