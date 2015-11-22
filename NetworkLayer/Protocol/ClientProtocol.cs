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

        IClientConnectionInterface connectionInterface = null;
        string clientName = "";

        ManualResetEvent sendingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent sendingThreadTerminate = new ManualResetEvent(false);

        ManualResetEvent receivingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent receivingThreadTerminate = new ManualResetEvent(false);

        Thread sendingWorker = null;
        Thread receivingWorker = null;

        Queue<Packet> sendingQueue = new Queue<Packet>();
        Queue<Packet> receivedQueue = new Queue<Packet>();

        private void SendingWorker()
        {
            while (true)
            {
                if (connectionInterface == null ||
                    !connectionInterface.IsConnected() ||
                    sendingThreadTerminate.WaitOne(0))
                    break;

                if (sendingQueue.Count == 0)
                    continue;

                Packet packetToSend = sendingQueue.Dequeue();
                connectionInterface.Send(packetToSend.SerializedData);
            }

            sendingThreadStopped.Set();
        }

        private void ReceivingWorker()
        {
            List<byte> receivedBuffer = new List<byte>(1024);
            BinaryFormatter b = new BinaryFormatter();
            const int headerSize = sizeof(PacketType) + sizeof(int);

            while (true)
            {
                if (!connectionInterface.IsConnected() ||
                    receivingThreadTerminate.WaitOne(0))
                    break;

                byte[] temporalBuffer = new byte[1024];
                int receivedCount = connectionInterface.Receive(temporalBuffer, 100);
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
                        default:
                            throw new Exception("Unknown protocol message");
                    }
                }
            }

            receivingThreadStopped.Set();
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
            if (!connectionInterface.IsConnected() ||
                type == PacketType.Unknown ||
                data.Length == 0)
                return false;

            Packet packetToSend = new Packet(type, data);
            sendingQueue.Enqueue(packetToSend);
            return true;
        }

        public ClientProtocol(string clientName)
        {
            this.clientName = clientName;
        }

        public bool Connect(string address)
        {
            Disconnect();

            connectionInterface = new InternetClientConnectionInterface();
            bool connectionStatus = connectionInterface.Connect(address, ProtocolShared.protocolPort);
            if (!connectionStatus)
                return false;

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            ClientInfoEventArgs data = new ClientInfoEventArgs { clientName = clientName };
            b.Serialize(m, data);
            if (!SendPacket(PacketType.ClientInfoMessage, m.GetBuffer()))
            {
                Disconnect();
                return false;
            }

            sendingThreadStopped.Reset();
            sendingWorker = new Thread(SendingWorker);
            sendingWorker.Start();

            receivingThreadStopped.Reset();
            receivingWorker = new Thread(ReceivingWorker);
            receivingWorker.Start();

            return true;
        }

        public void Disconnect()
        {
            if (sendingWorker != null)
            {
                sendingThreadTerminate.Set();
                sendingThreadStopped.WaitOne();
                sendingWorker.Abort();
                sendingWorker = null;
                sendingThreadTerminate.Reset();
            }

            if (receivingWorker != null)
            {
                receivingThreadTerminate.Set();
                receivingThreadStopped.WaitOne();
                receivingWorker.Abort();
                receivingWorker = null;
                receivingThreadTerminate.Reset();
            }

            if (connectionInterface != null)
            {
                connectionInterface.Disconnect();
                connectionInterface = null;
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
            Disconnect();
            sendingThreadStopped.Dispose();
            sendingThreadTerminate.Dispose();
            receivingThreadStopped.Dispose();
            receivingThreadTerminate.Dispose();
        }

        public event SensorsReceiveHandler SensorsReceive = delegate
        { };
        public event VoiceWindowReceiveHandler VoiceWindowReceive = delegate
        { };
        public event ChatMessageReceiveHandler ChatMessageReceive = delegate
        { };
    }
}
