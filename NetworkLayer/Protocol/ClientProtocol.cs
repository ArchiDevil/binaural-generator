using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLayer.Protocol
{
    public class ClientProtocol
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
            receivingThreadStopped.Set();
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

        public bool SendSignalSettings()
        {
            return false;
        }

        public bool SendVoiceWindow(byte[] voiceData)
        {
            if (voiceData == null || voiceData.Length == 0)
                return false;

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            VoiceWindowDataEventArgs data = new VoiceWindowDataEventArgs { data = voiceData };
            b.Serialize(m, data);
            return SendPacket(PacketType.VoiceMessage, m.GetBuffer());
        }

        public bool SendChatMessage(string message)
        {
            if (message == null || message.Length == 0)
                return false;

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            ClientChatMessageEventArgs data = new ClientChatMessageEventArgs { message = message };
            b.Serialize(m, data);
            return SendPacket(PacketType.ChatMessage, m.GetBuffer());
        }

        public event SensorsReceiveHandler SensorsReceive = delegate
        { };
        public event VoiceWindowReceiveHandler VoiceWindowReceive = delegate
        { };
        public event ChatMessageReceiveHandler ChatMessageReceive = delegate
        { };
    }
}
