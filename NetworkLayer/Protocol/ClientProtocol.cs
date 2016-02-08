using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace NetworkLayer.Protocol
{
    public sealed class ClientProtocol : IDisposable
    {
        public delegate void SensorsReceiveHandler(object sender, SensorsDataEventArgs e);
        public delegate void VoiceWindowReceiveHandler(object sender, VoiceWindowDataEventArgs e);
        public delegate void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e);

        IClientConnectionInterface _connectionInterface = null;
        string _clientName = "";

        ManualResetEvent _sendingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent _sendingThreadTerminate = new ManualResetEvent(false);

        ManualResetEvent _receivingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent _receivingThreadTerminate = new ManualResetEvent(false);

        Thread _sendingWorker = null;
        Thread _receivingWorker = null;

        Queue<Packet> _sendingQueue = new Queue<Packet>();
        Queue<Packet> _receivedQueue = new Queue<Packet>();

        int _sendingConnectionLostCount = 0;
        int _receivingConnectionLostCount = 0;

        private void SendingWorker()
        {
            while (true)
            {
                Thread.Yield();

                if (_connectionInterface == null ||
                    !_connectionInterface.IsConnected() ||
                    _sendingThreadTerminate.WaitOne(0))
                {
                    if (_sendingConnectionLostCount++ == 10)
                    {
                        break;
                    }
                    continue;
                }
                _sendingConnectionLostCount = 0;

                if (_sendingQueue.Count == 0)
                    continue;

                Packet packetToSend = _sendingQueue.Dequeue();
                _connectionInterface.Send(packetToSend.SerializedData);
            }

            _sendingThreadStopped.Set();
        }

        private void ReceivingWorker()
        {
            List<byte> receivedBuffer = new List<byte>(1024);
            BinaryFormatter b = new BinaryFormatter();
            const int headerSize = sizeof(PacketType) + sizeof(int);

            while (true)
            {
                Thread.Yield();

                if (!_connectionInterface.IsConnected() ||
                    _receivingThreadTerminate.WaitOne(0))
                {
                    if (_receivingConnectionLostCount++ == 10)
                    {
                        break;
                    }
                    continue;
                }
                _receivingConnectionLostCount = 0;

                byte[] temporalBuffer = new byte[1024];
                int receivedCount = _connectionInterface.Receive(temporalBuffer, 100);
                receivedBuffer.AddRange(temporalBuffer.Take(receivedCount));

                while (receivedBuffer.Count > 0)
                {
                    byte[] protocolHeader = receivedBuffer.Take(headerSize).ToArray();
                    PacketType type = (PacketType)protocolHeader[0];
                    int packetDataSize = BitConverter.ToInt32(protocolHeader, sizeof(PacketType));

                    if (receivedBuffer.Count < headerSize + packetDataSize)
                        break;

                    byte[] packetData = receivedBuffer.Skip(headerSize).Take(packetDataSize).ToArray();
                    receivedBuffer.RemoveRange(0, headerSize + packetDataSize);
                    MemoryStream m = new MemoryStream(packetData);
                    object deserialized = b.Deserialize(m);

                    switch (type)
                    {
                        case PacketType.ChatMessage:
                            ChatMessageReceive(this, (ClientChatMessageEventArgs)deserialized);
                            break;
                        case PacketType.VoiceMessage:
                            VoiceWindowReceive(this, (VoiceWindowDataEventArgs)deserialized);
                            break;
                        case PacketType.SensorsMessage:
                            SensorsReceive(this, (SensorsDataEventArgs)deserialized);
                            break;
                        case PacketType.ProtocolInfoMessage:
                            break;
                        case PacketType.ServerInfoMessage:
                            break;
                        default:
                            throw new Exception("Unknown protocol message");
                    }
                }
            }

            _receivingThreadStopped.Set();
        }

        private bool SendStruct(PacketType packetType, object structure)
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, structure);
            return SendPacket(packetType, m.GetBuffer());
        }

        private bool SendPacket(PacketType type, byte[] data)
        {
            if (_connectionInterface == null ||
                !_connectionInterface.IsConnected() ||
                type == PacketType.Unknown ||
                data.Length == 0)
                return false;

            Packet packetToSend = new Packet(type, data);
            _sendingQueue.Enqueue(packetToSend);
            return true;
        }

        public ClientProtocol(string clientName)
        {
            this._clientName = clientName;
        }

        public bool Connect(string address)
        {
            Disconnect();

            _connectionInterface = new InternetClientConnectionInterface();
            bool connectionStatus = _connectionInterface.Connect(address, ProtocolShared.protocolPort);
            if (!connectionStatus)
                return false;

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            ClientInfoEventArgs data = new ClientInfoEventArgs { clientName = _clientName };
            b.Serialize(m, data);
            if (!SendPacket(PacketType.ClientInfoMessage, m.GetBuffer()))
            {
                Disconnect();
                return false;
            }

            _sendingConnectionLostCount = 0;
            _sendingThreadStopped.Reset();
            _sendingWorker = new Thread(SendingWorker);
            _sendingWorker.Start();

            _receivingConnectionLostCount = 0;
            _receivingThreadStopped.Reset();
            _receivingWorker = new Thread(ReceivingWorker);
            _receivingWorker.Start();

            return true;
        }

        public void Disconnect()
        {
            if (_sendingWorker != null)
            {
                _sendingThreadTerminate.Set();
                _sendingThreadStopped.WaitOne();
                _sendingWorker.Abort();
                _sendingWorker = null;
                _sendingThreadTerminate.Reset();
            }

            if (_receivingWorker != null)
            {
                _receivingThreadTerminate.Set();
                _receivingThreadStopped.WaitOne();
                _receivingWorker.Abort();
                _receivingWorker = null;
                _receivingThreadTerminate.Reset();
            }

            if (_connectionInterface != null)
            {
                _connectionInterface.Disconnect();
                _connectionInterface = null;
            }
        }

        public bool SendSignalSettings(ChannelDescription[] channels, NoiseDescription noise)
        {
            if (channels == null || channels.Length == 0)
                return false;

            SettingsDataEventArgs data = new SettingsDataEventArgs { channels = channels, noise = noise };
            return SendStruct(PacketType.SettingsMessage, data);
        }

        public bool SendVoiceWindow(byte[] voiceData)
        {
            if (voiceData == null || voiceData.Length == 0)
                return false;

            VoiceWindowDataEventArgs data = new VoiceWindowDataEventArgs { data = voiceData };
            return SendStruct(PacketType.VoiceMessage, data);
        }

        public bool SendChatMessage(string message)
        {
            if (message == null || message.Length == 0)
                return false;

            ClientChatMessageEventArgs data = new ClientChatMessageEventArgs { message = message };
            return SendStruct(PacketType.ChatMessage, data);
        }

        public void Dispose()
        {
            _sendingThreadTerminate.Set();
            _receivingThreadTerminate.Set();

            Disconnect();

            _sendingThreadStopped.Dispose();
            _sendingThreadTerminate.Dispose();
            _receivingThreadStopped.Dispose();
            _receivingThreadTerminate.Dispose();
        }

        public event SensorsReceiveHandler SensorsReceive = delegate
        { };
        public event VoiceWindowReceiveHandler VoiceWindowReceive = delegate
        { };
        public event ChatMessageReceiveHandler ChatMessageReceive = delegate
        { };
    }
}
