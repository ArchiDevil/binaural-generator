using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace NetworkLayer.Protocol
{
    public sealed class ServerProtocol : IDisposable
    {
        IServerConnectionInterface connectionInterface = null;
        Thread sendingWorker = null;
        Thread receivingWorker = null;

        public delegate void ClientConnectionHandler(object sender, ClientInfoEventArgs e);
        public delegate void SettingsReceiveHandler(object sender, SettingsDataEventArgs e);
        public delegate void VoiceWindowReceiveHandler(object sender, VoiceWindowDataEventArgs e);
        public delegate void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e);

        ManualResetEvent sendingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent sendingThreadTerminate = new ManualResetEvent(false);

        ManualResetEvent receivingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent receivingThreadTerminate = new ManualResetEvent(false);

        Queue<Packet> sendingQueue = new Queue<Packet>();
        Queue<Packet> receivedQueue = new Queue<Packet>();

        string serverName = null;

        public ServerProtocol(string serverName)
        {
            this.serverName = serverName;
        }

        private void SendingWorker()
        {
            while (true)
            {
                Thread.Yield();

                if (!connectionInterface.IsListening() ||
                    !connectionInterface.IsClientConnected() ||
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
                Thread.Yield();

                if (!connectionInterface.IsListening() ||
                    !connectionInterface.IsClientConnected() ||
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
                        case PacketType.SettingsMessage:
                            SettingsReceive(this, (SettingsDataEventArgs)deserialized);
                            break;
                        case PacketType.VoiceMessage:
                            VoiceWindowReceive(this, (VoiceWindowDataEventArgs)deserialized);
                            break;
                        case PacketType.ProtocolInfoMessage:
                            break;
                        default:
                            throw new Exception("Unknown protocol message");
                    }
                }
            }

            receivingThreadStopped.Set();
        }

        public bool Bind()
        {
            connectionInterface = new InternetServerConnectionInterface();
            if (connectionInterface == null)
                return false;

            connectionInterface.ClientConnected += ClientConnectedEvent;
            bool result = connectionInterface.StartListening(ProtocolShared.protocolPort);

            return result;
        }

        public bool Bind(string host)
        {
            connectionInterface = new InternetServerConnectionInterface();
            if (connectionInterface == null)
                return false;

            connectionInterface.ClientConnected += ClientConnectedEvent;
            bool result = connectionInterface.StartListening(host, ProtocolShared.protocolPort);

            return result;
        }

        public void Stop()
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
                lock (connectionInterface)
                {
                    connectionInterface.Shutdown();
                    connectionInterface = null;
                }
            }
        }

        public bool SendSensorsData(double temperatureValue, double skinResistanceValue, double motionValue, double pulseValue)
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();

            SensorsDataEventArgs data = new SensorsDataEventArgs
            {
                motionValue = motionValue,
                skinResistanceValue = skinResistanceValue,
                pulseValue = pulseValue,
                temperatureValue = temperatureValue
            };

            b.Serialize(m, data);
            return SendPacket(PacketType.SensorsMessage, m.GetBuffer());
        }

        public bool SendVoiceWindow(byte[] voiceData)
        {
            if (voiceData == null)
                return false;

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            VoiceWindowDataEventArgs data = new VoiceWindowDataEventArgs { data = voiceData };
            b.Serialize(m, data);
            return SendPacket(PacketType.VoiceMessage, m.GetBuffer());
        }

        public bool SendChatMessage(string message)
        {
            if (message == null)
                return false;

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            ClientChatMessageEventArgs msg = new ClientChatMessageEventArgs();
            msg.message = message;
            b.Serialize(m, msg);
            return SendPacket(PacketType.ChatMessage, m.GetBuffer());
        }

        public void Dispose()
        {
            Stop();

            sendingThreadStopped.Dispose();
            sendingThreadTerminate.Dispose();
            receivingThreadStopped.Dispose();
            receivingThreadTerminate.Dispose();
        }

        private bool SendPacket(PacketType type, byte[] data)
        {
            if (!connectionInterface.IsListening() ||
                !connectionInterface.IsClientConnected() ||
                type == PacketType.Unknown ||
                data.Length == 0)
                return false;

            Packet packetToSend = new Packet(type, data);
            sendingQueue.Enqueue(packetToSend);
            return true;
        }

        private void ClientConnectedEvent(object sender, EventArgs e)
        {
            ProtocolInfo protocolInfo = new ProtocolInfo();
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, protocolInfo);

            SendPacket(PacketType.ProtocolInfoMessage, stream.GetBuffer());

            lock (connectionInterface)
            {
                byte[] buffer = new byte[1024];
                int count = connectionInterface.Receive(buffer, 5000);
                if (count > 0)
                {
                    //check info here
                    if (buffer[0] != (byte)PacketType.ClientInfoMessage)
                        return;

                    int packetSize = BitConverter.ToInt32(buffer, 1);
                    if (packetSize <= 0)
                        return;

                    ClientInfoEventArgs info = new ClientInfoEventArgs();
                    info.clientName = Encoding.UTF8.GetString(buffer, 5, packetSize);
                    ClientConnected(this, info);

                    ServerInfoEventArgs serverInfo = new ServerInfoEventArgs { serverName = this.serverName };
                    stream = new MemoryStream();
                    formatter.Serialize(stream, serverInfo);
                    SendPacket(PacketType.ServerInfoMessage, stream.GetBuffer());

                    // everything is ok, start working
                    sendingThreadStopped.Reset();
                    sendingWorker = new Thread(SendingWorker);
                    sendingWorker.Start();

                    receivingThreadStopped.Reset();
                    receivingWorker = new Thread(ReceivingWorker);
                    receivingWorker.Start();
                }
            }
        }

        public event ClientConnectionHandler ClientConnected = delegate
        { };
        public event SettingsReceiveHandler SettingsReceive = delegate
        { };
        public event VoiceWindowReceiveHandler VoiceWindowReceive = delegate
        { };
        public event ChatMessageReceiveHandler ChatMessageReceive = delegate
        { };
    }
}
