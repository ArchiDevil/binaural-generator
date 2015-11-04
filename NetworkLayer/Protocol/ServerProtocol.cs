using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLayer
{
    public class ServerProtocol : IDisposable
    {
        IServerConnectionInterface connectionInterface = null;
        Thread sendingWorker = null;
        Thread receivingWorker = null;

        public const int protocolPort = 31012;
        public delegate void ClientConnectionHandler(object sender, ClientInfoEventArgs e);
        public delegate void SettingsReceiveHandler(object sender, SettingsDataEventArgs e);
        public delegate void VoiceWindowReceiveHandler(object sender, VoiceWindowDataEventArgs e);
        public delegate void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e);

        ManualResetEvent sendingThreadStopped = new ManualResetEvent(false);
        ManualResetEvent receivingThreadStopped = new ManualResetEvent(false);

        Queue<Packet> sendingQueue = new Queue<Packet>();
        Queue<Packet> receivedQueue = new Queue<Packet>();

        bool sendingTerminate = false;
        bool receivingTerminate = false;

        private void SendingWorker()
        {
            while (true)
            {
                if (connectionInterface == null ||
                    !connectionInterface.IsListening() ||
                    !connectionInterface.IsClientConnected() ||
                    sendingTerminate)
                    break;

                if (sendingQueue.Count == 0)
                    continue;

                Packet packetToSend = sendingQueue.Dequeue();
                connectionInterface.Send(packetToSend.SerializedData);
            }

            sendingTerminate = false;
            sendingThreadStopped.Set();
        }

        private void ReceivingWorker()
        {
            List<byte> receivedBuffer = new List<byte>(1024);
            while (true)
            {
                if (connectionInterface == null ||
                    !connectionInterface.IsListening() ||
                    !connectionInterface.IsClientConnected() ||
                    receivingTerminate)
                    break;

                byte[] temporalBuffer = new byte[1024];
                int receivedCount = connectionInterface.Receive(temporalBuffer);
                receivedBuffer.AddRange(temporalBuffer.Take(receivedCount));

                while (receivedBuffer.Count > 0)
                {
                    PacketType type = (PacketType)receivedBuffer[0];
                    int packetDataSize = BitConverter.ToInt32(receivedBuffer.Skip(1).Take(sizeof(int)).ToArray(), 0);

                    if (receivedBuffer.Count < 1 + sizeof(int) + packetDataSize)
                        break;

                    byte[] packetData = receivedBuffer.Skip(1 + sizeof(int)).Take(packetDataSize).ToArray();
                    receivedBuffer.RemoveRange(0, 1 + sizeof(int) + packetDataSize);
                    MemoryStream m = new MemoryStream(packetData);
                    BinaryFormatter b = new BinaryFormatter();

                    switch (type)
                    {
                        case PacketType.ChatMessage:
                            break;
                        case PacketType.ClientInfoMessage:
                            ClientInfoReceived((ClientInfoEventArgs)b.Deserialize(m));
                            break;
                        case PacketType.ProtocolInfoMessage:
                            break;
                        case PacketType.SensorsMessage:
                            break;
                        case PacketType.SettingsMessage:
                            break;
                        case PacketType.VoiceMessage:
                            break;
                        case PacketType.Unknown:
                        default:
                            throw new Exception("Unknown protocol message");
                    }
                }
            }

            receivingTerminate = false;
            receivingThreadStopped.Set();
        }

        public bool Bind(string host)
        {
            connectionInterface = new InternetServerConnectionInterface();
            if (connectionInterface == null)
                return false;

            connectionInterface.ClientConnected += ClientConnectedEvent;
            bool result = connectionInterface.StartListening(host, protocolPort);

            return result;
        }

        public void Stop()
        {
            if (sendingWorker != null)
            {
                sendingTerminate = true;
                sendingThreadStopped.WaitOne();
                sendingWorker.Abort();
                sendingWorker = null;
            }

            if (receivingWorker != null)
            {
                receivingTerminate = true;
                receivingThreadStopped.WaitOne();
                receivingWorker.Abort();
                receivingWorker = null;
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

        public bool SendSensorsData(SensorsDataEventArgs data)
        {
            if (data == null)
                return false;

            // 4 double fields
            byte[] packetData = new byte[sizeof(double) * 4];
            BitConverter.GetBytes(data.temperatureValue).CopyTo(packetData, sizeof(double) * 0);
            BitConverter.GetBytes(data.skinResistanceValue).CopyTo(packetData, sizeof(double) * 1);
            BitConverter.GetBytes(data.motionValue).CopyTo(packetData, sizeof(double) * 2);
            BitConverter.GetBytes(data.pulseValue).CopyTo(packetData, sizeof(double) * 3);

            return SendPacket(PacketType.SensorsMessage, packetData);
        }

        public bool SendVoiceWindow(VoiceWindowDataEventArgs data)
        {
            if (data == null || data.data == null)
                return false;

            return SendPacket(PacketType.VoiceMessage, data.data);
        }

        public bool SendChatMessage(string message)
        {
            if (message == null)
                return false;

            byte[] msgData = Encoding.UTF8.GetBytes(message);
            return SendPacket(PacketType.ChatMessage, msgData);
        }

        public void Dispose()
        {
            Stop();
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
                    if(buffer[0] != (byte)PacketType.ClientInfoMessage)
                        return;

                    int packetSize = BitConverter.ToInt32(buffer, 1);
                    if (packetSize <= 0)
                        return;

                    ClientInfoEventArgs info = new ClientInfoEventArgs();
                    info.clientName = Encoding.UTF8.GetString(buffer, 5, packetSize);
                    ClientConnected(this, info);

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

        ClientInfoEventArgs info = null;

        private void ClientInfoReceived(ClientInfoEventArgs e)
        {
            info = e;
        }

        public event ClientConnectionHandler ClientConnected = delegate { };
        public event SettingsReceiveHandler SettingsReceive = delegate { };
        public event VoiceWindowReceiveHandler VoiceWindowReceive = delegate { };
        public event ChatMessageReceiveHandler ChatMessageReceive = delegate { };
    }
}
