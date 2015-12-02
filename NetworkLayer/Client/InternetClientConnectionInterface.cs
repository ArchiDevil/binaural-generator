using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkLayer
{
    public sealed class InternetClientConnectionInterface : IClientConnectionInterface, IDisposable
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
                //Debug.Assert(false, e.Message);
                throw e;
            }

            return true;
        }

        public void Disconnect()
        {
            if (sender != null)
            {
                if (sender.IsConnected() && sender.Connected)
                {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                sender = null;
            }
        }

        public int Send(byte[] data)
        {
            if (sender == null)
                return 0;

            int count = 0;
            if (sender.Poll(-1, SelectMode.SelectWrite) && sender.IsConnected())
                count = sender.Send(data);
            return count;
        }

        public int Receive(byte[] data)
        {
            if (sender == null)
                return 0;

            int count = 0;
            if (sender.Poll(-1, SelectMode.SelectRead) && sender.IsConnected())
                count = sender.Receive(data);
            return count;
        }

        public int Receive(byte[] data, int millisecondsTimeout)
        {
            if (sender == null)
                return 0;

            int count = 0;
            if (sender.Poll(millisecondsTimeout * 1000, SelectMode.SelectRead) && sender.IsConnected())
                count = sender.Receive(data);
            return count;
        }

        public int Receive(byte[] data, int offset, int size)
        {
            if (sender == null)
                return 0;

            int count = 0;
            if (sender.Poll(-1, SelectMode.SelectWrite) && sender.IsConnected())
                count = sender.Receive(data, offset, size, SocketFlags.None);
            return count;
        }

        public bool IsConnected()
        {
            return sender != null && sender.IsConnected();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
