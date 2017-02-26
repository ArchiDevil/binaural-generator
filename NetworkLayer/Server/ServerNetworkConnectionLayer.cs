using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using NetworkLayer.ConnectionLayerShared;

namespace NetworkLayer.Server
{
    public class ServerNetworkConnectionLayer : IServerConnectionLayer, IDisposable
    {
        public event ServerClientConnectionHandler ClientConnected = delegate { };
        public event ServerClientDisconnectionHandler ClientDisconnected = delegate { };
        public event ServerClientDisconnectionHandler ConnectionLost = delegate { };
        public event PacketReceivedHander PacketReceived = delegate { };

        Thread _sendingWorker = null;
        Thread _receivingWorker = null;

        ManualResetEvent _sendingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent _sendingThreadTerminate = new ManualResetEvent(false);

        ManualResetEvent _receivingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent _receivingThreadTerminate = new ManualResetEvent(false);

        ManualResetEvent _errorDiscovered = new ManualResetEvent(false);

        Queue<Packet> _sendingQueue = new Queue<Packet>();

        List<Socket> _listenerSockets = new List<Socket>();
        Socket _client = null;

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.OperationAborted)
            {
                // we don't need to set new handler for accepting new connection
                // it's just aborted by shutting down
                return;
            }

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += AcceptCompleted;
            (sender as Socket).AcceptAsync(args);

            if (_client != null)
            {
                if (_client.IsConnected())
                    _client.Shutdown(SocketShutdown.Both);
                _client.Close();
                _client = null;
            }

            _client = e.AcceptSocket;

            if (!_client.IsConnected())
                return;

            try
            {
                byte[] buffer = new byte[1024];
                int received = _client.Receive(buffer);
                string message = Encoding.ASCII.GetString(buffer, 0, received);
                if (message.IndexOf(ConnectionLayerConstants.handshakeString) == -1)
                    throw new Exception("Handshake failed");

                buffer = Encoding.ASCII.GetBytes(ConnectionLayerConstants.handshakeString);
                if (_client.Send(buffer) == 0)
                    throw new Exception("Handshake failed");

                // everything is ok, start workers
                _sendingThreadStopped.Reset();
                _sendingWorker = new Thread(SendingWorker);
                _sendingWorker.Start();

                _receivingThreadStopped.Reset();
                _receivingWorker = new Thread(ReceivingWorker);
                _receivingWorker.Start();

                ClientConnected(this, new ClientConnectedEventArgs { clientAddress = _client.RemoteEndPoint.Serialize().ToString() });
            }
            catch (Exception exc)
            {
                Debug.Print(exc.Message);
                StopInternal(true);
            }
        }

        private void SendingWorker()
        {
            while (!_sendingThreadTerminate.WaitOne(0))
            {
                try
                {
                    if (!IsListening() || !IsClientConnected())
                        throw new Exception("Connection lost");

                    // nothing to send
                    if (_sendingQueue.Count == 0)
                    {
                        Thread.Yield();
                        continue;
                    }

                    Packet packetToSend = _sendingQueue.Dequeue();
                    int bytesSent = _client.Send(packetToSend.SerializedData);
                    if (bytesSent == 0)
                        throw new Exception("Socket failed to send data");
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                    ConnectionLost(this);
                    _errorDiscovered.Set();
                    break;
                }
            }

            _sendingThreadStopped.Set();
        }

        private void ReceivingWorker()
        {
            List<byte> receivedBuffer = new List<byte>(1024);
            byte[] temporalBuffer = new byte[1024];

            while (!_receivingThreadTerminate.WaitOne(0))
            {
                try
                {
                    if (!IsListening() || !IsClientConnected())
                        throw new Exception("Connection lost");

                    if(_client.Available == 0)
                    {
                        Thread.Yield();
                        continue;
                    }

                    int receivedCount = _client.Receive(temporalBuffer);
                    receivedBuffer.AddRange(temporalBuffer.Take(receivedCount));
                    SharedFunctions.ExtractPackets(ref receivedBuffer, out bool disconnection, x => PacketReceived(this, x));
                    if (disconnection)
                    {
                        ClientDisconnected(this);
                        _receivingThreadTerminate.Set();
                        _sendingThreadTerminate.Set();
                        _client.Close();
                        _client = null;
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                    ConnectionLost(this);
                    _errorDiscovered.Set();
                    break;
                }
            }

            _receivingThreadStopped.Set();
        }

        private bool SendPacket(PacketType packetType, byte[] data)
        {
            if (!IsListening() || !IsClientConnected())
                return false;

            Packet packetToSend = new Packet(packetType, data);
            _sendingQueue.Enqueue(packetToSend);
            return true;
        }

        public bool Bind(ushort port)
        {
            if (IsListening() || IsClientConnected())
                throw new InvalidOperationException("Already bound");

            IPAddress[] addresses = null;
            IPAddress[] localAddresses = null;
            localAddresses = Dns.GetHostEntry("localhost").AddressList;
            addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;

            IPAddress[] totalAddresses = new IPAddress[addresses.Length + localAddresses.Length];
            addresses.CopyTo(totalAddresses, 0);
            localAddresses.CopyTo(totalAddresses, addresses.Length);

            if (totalAddresses.Length == 0)
                return false;

            try
            {
                foreach (var address in totalAddresses)
                {
                    if (address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    IPEndPoint localEndPoint = new IPEndPoint(address, port);
                    Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    listenerSocket.Bind(localEndPoint);
                    listenerSocket.Listen(1);

                    SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                    e.Completed += AcceptCompleted;
                    listenerSocket.AcceptAsync(e);

                    _listenerSockets.Add(listenerSocket);
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);

                foreach (var socket in _listenerSockets)
                    socket.Close();

                _listenerSockets.Clear();
                return false;
            }

            return true;
        }

        public bool IsClientConnected()
        {
            return _client != null;
        }

        public bool IsListening()
        {
            return _listenerSockets.Count > 0;
        }

        public bool SendData(byte[] data)
        {
            if(_errorDiscovered.WaitOne(0))
                throw new Exception("Connection layer failed with previous sending/receiving");

            if (!IsListening() || !IsClientConnected() || data == null)
                return false;

            return SendPacket(PacketType.UserDataPacket, data);
        }

        private bool StopInternal(bool error = false)
        {
            if (!error)
                SendPacket(PacketType.FinishingPacket, new byte[1]);

            if (_sendingWorker != null)
            {
                _sendingThreadTerminate.Set();
                if (!_sendingThreadStopped.WaitOne(1000))
                    throw new Exception("Unable to stop connection layer");

                _sendingWorker.Abort();
                _sendingWorker = null;
                _sendingThreadTerminate.Reset();
            }

            if (_receivingWorker != null)
            {
                _receivingThreadTerminate.Set();
                if (!_receivingThreadStopped.WaitOne(1000))
                    throw new Exception("Unable to stop connection layer");

                _receivingWorker.Abort();
                _receivingWorker = null;
                _receivingThreadTerminate.Reset();
            }

            if (_client != null)
            {
                if (_client.IsConnected())
                    _client.Shutdown(SocketShutdown.Both);
                _client.Close();
                _client = null;
            }

            try
            {
                foreach (var socket in _listenerSockets)
                {
                    socket.Close();
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                throw;
            }

            _listenerSockets.Clear();
            _sendingQueue.Clear();

            return true;
        }

        public bool Stop()
        {
            return StopInternal(false);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
