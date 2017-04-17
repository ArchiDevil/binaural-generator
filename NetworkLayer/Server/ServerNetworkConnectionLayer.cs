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
        public event ServerClientConnectionHandler ClientConnected;
        public event ServerClientDisconnectionHandler ClientDisconnected;
        public event ServerClientDisconnectionHandler ConnectionLost;
        public event PacketReceivedHander PacketReceived;

        Thread _socketWorker = null;

        Queue<Packet> _sendingQueue = new Queue<Packet>();
        Queue<Packet> _receivedQueue = new Queue<Packet>();

        ManualResetEvent _workerThreadStopped = new ManualResetEvent(false);
        ManualResetEvent _workerThreadTerminate = new ManualResetEvent(false);
        ManualResetEvent _errorDiscovered = new ManualResetEvent(false);
        List<Socket> _listenerSockets = new List<Socket>();
        Socket _sender = null;

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

            if (_sender != null)
            {
                if (_sender.IsConnected())
                    _sender.Shutdown(SocketShutdown.Both);
                _sender.Close();
                _sender = null;
            }

            _sender = e.AcceptSocket;

            if (!_sender.IsConnected())
                return;

            try
            {
                byte[] buffer = new byte[1024];
                int received = _sender.Receive(buffer);
                string message = Encoding.ASCII.GetString(buffer, 0, received);
                if (message.IndexOf(ConnectionLayerConstants.handshakeString) == -1)
                    throw new Exception("Handshake failed");

                buffer = Encoding.ASCII.GetBytes(ConnectionLayerConstants.handshakeString);
                if (_sender.Send(buffer) == 0)
                    throw new Exception("Handshake failed");

                // everything is OK, start workers
                _workerThreadStopped.Reset();
                _socketWorker = new Thread(SocketWorker);
                _socketWorker.Start();

                ClientConnected?.Invoke(this, new ClientConnectedEventArgs { clientAddress = _sender.RemoteEndPoint.Serialize().ToString() });
            }
            catch (Exception exc)
            {
                Debug.Print(exc.Message);
                StopInternal(true);
            }
        }

        private void SocketWorker()
        {
            List<byte> receivedBuffer = new List<byte>(1024);
            byte[] temporalBuffer = new byte[1024];

            while (true)
            {
                try
                {
                    bool error = false;
                    if (!_sender.Poll(1000 * 100, SelectMode.SelectWrite))
                    {
                        Debug.Print("Sending socket closed unexpectedly");
                        error = true;
                    }

                    if (_sendingQueue.Count != 0 && !error)
                    {
                        while (_sendingQueue.Count > 0)
                        {
                            Packet packetToSend = _sendingQueue.Dequeue();
                            // TODO: handle sending correctly
                            int sentBytesCount = _sender.Send(packetToSend.SerializedData);
                            if (sentBytesCount == 0)
                            {
                                Debug.Print("Sending socket was unable to send data");
                                error = true;
                                break;
                            }
                        }
                    }
                    else if (_sender.Available > 0 && !error)
                    {
                        while (_sender.Available > 0)
                        {
                            int receivedCount = _sender.Receive(temporalBuffer);
                            receivedBuffer.AddRange(temporalBuffer.Take(receivedCount));
                            SharedFunctions.ExtractPackets(ref receivedBuffer, out bool disconnection, x => PacketReceived?.Invoke(this, x));
                            if (disconnection)
                            {
                                ClientDisconnected?.Invoke(this);
                                _workerThreadTerminate.Set();
                                _workerThreadStopped.Set();
                                _sender.Close();
                                _sender = null;
                                return;
                            }
                        }
                    }
                    else if (!error)
                    {
                        Thread.Yield();
                        continue;
                    }

                    if (error)
                    {
                        _errorDiscovered.Set();
                        break;
                    }

                    if (_workerThreadTerminate.WaitOne(0))
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                    ConnectionLost?.Invoke(this);
                    _errorDiscovered.Set();
                    break;
                }
            }

            _workerThreadStopped.Set();
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
            return _sender != null;
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

            if (_socketWorker != null)
            {
                _workerThreadTerminate.Set();
                if (!_workerThreadStopped.WaitOne(1000))
                    throw new Exception("Unable to stop connection layer");

                _socketWorker.Abort();
                _socketWorker = null;
                _workerThreadTerminate.Reset();
            }

            if (_sender != null)
            {
                if (_sender.IsConnected())
                    _sender.Shutdown(SocketShutdown.Both);
                _sender.Close();
                _sender = null;
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
            StopInternal();

            _workerThreadStopped.Dispose();
            _workerThreadTerminate.Dispose();
        }
    }
}
