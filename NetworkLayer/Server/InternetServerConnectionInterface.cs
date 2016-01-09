using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLayer
{
    public sealed class InternetServerConnectionInterface : IServerConnectionInterface, IDisposable
    {
        private ushort _port = 0;
        private List<Socket> _listenerSockets = null;
        private Socket _client = null;

        public event ClientConnectedHandler ClientConnected = delegate
        { };

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }

            _client = e.AcceptSocket;

            if (!_client.Connected)
                return;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += AcceptCompleted;
            (sender as Socket).AcceptAsync(args);

            try
            {
                byte[] buffer = new byte[1024];
                int received = _client.Receive(buffer);
                string message = Encoding.ASCII.GetString(buffer, 0, received);
                if (message.IndexOf("<EOF>") == -1)
                {
                    throw new Exception("Wrong startup message");
                }

                buffer = Encoding.ASCII.GetBytes("<EOF>");
                int sent = _client.Send(buffer);
                if (sent == 0)
                {
                    throw new Exception("Wrong startup message");
                }

                ClientConnected(this, new EventArgs());
            }
            catch (Exception exc)
            {
                Debug.Assert(false, exc.Message);
                // unrecoverable error =(
                Shutdown();
            }
        }

        public bool StartListening(ushort port)
        {
            Shutdown();
            _port = port;

            IPAddress[] addresses = null;
            IPAddress[] localAddresses = null;
            localAddresses = Dns.GetHostEntry("localhost").AddressList;
            addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;

            IPAddress[] totalAddresses = new IPAddress[addresses.Length + localAddresses.Length];
            addresses.CopyTo(totalAddresses, 0);
            localAddresses.CopyTo(totalAddresses, addresses.Length);

            if (_listenerSockets == null)
                _listenerSockets = new List<Socket>();

            if (localAddresses.Length == 0)
                return false;

            foreach (var address in totalAddresses)
            {
                if (address.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                IPEndPoint localEndPoint = new IPEndPoint(address, port);
                Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listenerSockets.Add(listenerSocket);
                try
                {
                    listenerSocket.Bind(localEndPoint);
                    listenerSocket.Listen(1);
                    SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                    e.Completed += AcceptCompleted;
                    listenerSocket.AcceptAsync(e);
                }
                catch (Exception e)
                {
                    Debug.Assert(false, e.Message);
                    return false;
                }
            }

            return true;
        }

        public void Shutdown()
        {
            if (_client != null)
            {
                if (_client.IsConnected() && _client.Connected)
                {
                    _client.Shutdown(SocketShutdown.Both);
                    _client.Close();
                }
                _client = null;
            }

            if (_listenerSockets != null)
            {
                foreach (var socket in _listenerSockets)
                {
                    socket.Dispose();
                }

                _listenerSockets.Clear();
                _listenerSockets = null;
            }
        }

        public int Receive(byte[] data)
        {
            if (_client == null)
                return 0;

            int count = 0;
            try
            {
                if (_client.Poll(-1, SelectMode.SelectRead) && _client.IsConnected())
                    count = _client.Receive(data);
            }
            catch (SocketException e)
            {
                Debug.Assert(false, e.Message);
                _client.Close();
                _client = null;
            }
            return count;
        }

        public int Receive(byte[] data, int millisecondsTimeout)
        {
            if (_client == null)
                return 0;

            int count = 0;
            try
            {
                if (_client.Poll(millisecondsTimeout * 1000, SelectMode.SelectRead) && _client.IsConnected())
                    count = _client.Receive(data);
            }
            catch (SocketException e)
            {
                Debug.Assert(false, e.Message);
                _client.Close();
                _client = null;
            }
            return count;
        }

        public int Send(byte[] data)
        {
            if (_client == null)
                return 0;

            int count = 0;
            try
            {
                if (_client.IsConnected())
                    count = _client.Send(data);
            }
            catch (SocketException e)
            {
                Debug.Assert(false, e.Message);
                _client.Close();
                _client = null;
            }
            return count;
        }

        public bool IsListening()
        {
            if (_listenerSockets == null)
                return false;

            return _listenerSockets.Count > 0;
        }

        public bool IsClientConnected()
        {
            return _client != null;
        }

        public void Dispose()
        {
            Shutdown();
        }
    }
}
