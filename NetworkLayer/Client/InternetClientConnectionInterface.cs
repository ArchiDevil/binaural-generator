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
        string _address = string.Empty;
        int _port = -1;
        Socket _sender = null;

        public bool Connect(string address, int port)
        {
            Disconnect();

            _address = address;
            _port = port;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(address);
            IPAddress ipAddress = ipHostInfo.AddressList.Where((a, i) => a.AddressFamily == AddressFamily.InterNetwork).First();

            if (ipAddress == null)
                return false;

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _sender.Connect(remoteEP);

                byte[] msg = Encoding.ASCII.GetBytes("<EOF>");
                _sender.Send(msg);

                // waiting for the response...
                msg = new byte[1024];
                int count = _sender.Receive(msg);

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
            if (_sender != null)
            {
                if (_sender.IsConnected() && _sender.Connected)
                {
                    _sender.Shutdown(SocketShutdown.Both);
                    _sender.Close();
                }
                _sender = null;
            }
        }

        public int Send(byte[] data)
        {
            if (_sender == null)
                return 0;

            int count = 0;
            if (_sender.Poll(-1, SelectMode.SelectWrite) && _sender.IsConnected())
                count = _sender.Send(data);
            return count;
        }

        public int Receive(byte[] data)
        {
            if (_sender == null)
                return 0;

            int count = 0;
            if (_sender.Poll(-1, SelectMode.SelectRead) && _sender.IsConnected())
                count = _sender.Receive(data);
            return count;
        }

        public int Receive(byte[] data, int millisecondsTimeout)
        {
            if (_sender == null)
                return 0;

            int count = 0;
            if (_sender.Poll(millisecondsTimeout * 1000, SelectMode.SelectRead) && _sender.IsConnected())
                count = _sender.Receive(data);
            return count;
        }

        public int Receive(byte[] data, int offset, int size)
        {
            if (_sender == null)
                return 0;

            int count = 0;
            if (_sender.Poll(-1, SelectMode.SelectWrite) && _sender.IsConnected())
                count = _sender.Receive(data, offset, size, SocketFlags.None);
            return count;
        }

        public bool IsConnected()
        {
            return _sender != null && _sender.IsConnected();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
