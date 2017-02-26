using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using NetworkLayer.ConnectionLayerShared;

namespace NetworkLayer.Client
{
    public class ClientNetworkConnectionLayer : IClientConnectionLayer, IDisposable
    {
        public event ClientConnectionHandler ConnectionEstablished = delegate { };
        public event ClientDisconnectionHandler ConnectionLost = delegate { };
        public event ClientDisconnectionHandler ConnectionFinished = delegate { };
        public event PacketReceivedHander PacketReceived = delegate { };

        ManualResetEvent _workerThreadStopped = new ManualResetEvent(false);
        ManualResetEvent _workerThreadTerminate = new ManualResetEvent(false);
        ManualResetEvent _errorDiscovered = new ManualResetEvent(false);
        Thread _socketWorker = null;

        Queue<Packet> _sendingQueue = new Queue<Packet>();
        Queue<Packet> _receivedQueue = new Queue<Packet>();

        Socket _sender = null;
        string _address = string.Empty;

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
                        while(_sender.Available > 0)
                        {
                            int receivedCount = _sender.Receive(temporalBuffer);
                            receivedBuffer.AddRange(temporalBuffer.Take(receivedCount));
                            SharedFunctions.ExtractPackets(ref receivedBuffer, out bool disconnection, x => PacketReceived(this, x));
                            if (disconnection)
                            {
                                ConnectionFinished(this);
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
                    ConnectionLost(this);
                    _errorDiscovered.Set();
                    break;
                }
            }

            _workerThreadStopped.Set();
        }

        public bool Connect(string address, ushort port)
        {
            if (IsConnected())
            {
                throw new InvalidOperationException("Already connected");
            }

            _address = address;

            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(address);
                IPAddress ipAddress = ipHostInfo.AddressList.Where((a, i) => a.AddressFamily == AddressFamily.InterNetwork).First();

                if (ipAddress == null)
                    return false;

                _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sender.Connect(ipAddress, port);

                // sending handshake
                byte[] msg = Encoding.ASCII.GetBytes(ConnectionLayerConstants.handshakeString);
                _sender.Send(msg);

                // waiting for the response...
                msg = new byte[32];
                int count = _sender.Receive(msg);

                string response = Encoding.ASCII.GetString(msg, 0, count);
                if (response.IndexOf(ConnectionLayerConstants.handshakeString) == -1)
                {
                    if (_sender.IsConnected())
                    {
                        _sender.Shutdown(SocketShutdown.Both);
                    }
                    _sender.Close();
                    _sender = null;
                    return false;
                }

                ConnectionEstablished(this);
            }
            catch (Exception exc)
            {
                Debug.Print(exc.Message);
                if (_sender != null)
                {
                    if (_sender.IsConnected() && _sender.Connected)
                        _sender.Shutdown(SocketShutdown.Both);
                    _sender.Close();
                    _sender = null;
                }
                return false;
            }

            // start sender/receiver
            _workerThreadStopped.Reset();
            _socketWorker = new Thread(SocketWorker);
            _socketWorker.Start();

            return true;
        }

        private void SendPacket(PacketType packetType, byte[] data)
        {
            _sendingQueue.Enqueue(new Packet(packetType, data));
        }

        public bool Disconnect()
        {
            SendPacket(PacketType.FinishingPacket, new byte[1]);

            if (_socketWorker != null)
            {
                _workerThreadTerminate.Set();
                if (!_workerThreadStopped.WaitOne(10000))
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

            return true;
        }

        public bool IsConnected()
        {
            return _sender != null
                && !_workerThreadStopped.WaitOne(0);
        }

        public bool SendData(byte[] data)
        {
            if (_errorDiscovered.WaitOne(0))
                throw new Exception("Connection layer failed with previous sending/receiving");

            if (!IsConnected() || data == null || _socketWorker == null || _workerThreadStopped.WaitOne(0))
                return false;

            SendPacket(PacketType.UserDataPacket, data);
            return true;
        }

        public void Dispose()
        {
            Disconnect();

            _workerThreadStopped.Dispose();
            _workerThreadTerminate.Dispose();
        }
    }
}
