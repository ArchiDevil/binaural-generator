using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NetworkLayer.ProtocolShared;
using NetworkLayer.Server;

namespace NetworkLayer
{
    public sealed class ServerProtocol
    {
        public delegate void ClientConnectionHandler(object sender, ClientInfoEventArgs e);
        public delegate void ClientDisconnectionHandler(object sender, EventArgs e);
        public delegate void SettingsReceiveHandler(object sender, SettingsDataEventArgs e);
        public delegate void VoiceWindowReceiveHandler(object sender, VoiceWindowDataEventArgs e);
        public delegate void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e);

        public event ClientConnectionHandler ClientConnected;
        public event ClientDisconnectionHandler ClientDisconnected;
        public event ClientDisconnectionHandler ConnectionLost;
        public event SettingsReceiveHandler SettingsReceived;
        public event VoiceWindowReceiveHandler VoiceWindowReceived;
        public event ChatMessageReceiveHandler ChatMessageReceived;

        string _serverName = string.Empty;

        IServerConnectionLayer connectionLayer = new ServerNetworkConnectionLayer();

        private bool SendData(ProtocolPacketType type, object data)
        {
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();

            ProtocolPacket packet = new ProtocolPacket
            {
                packetType = type,
                serializedData = data
            };

            b.Serialize(m, packet);

            return connectionLayer.SendData(m.GetBuffer());
        }

        private void ConnectionLayer_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            ProtocolInfo protocolInfo = new ProtocolInfo();
            SendData(ProtocolPacketType.ProtocolInfoPacket, protocolInfo);

            ServerInfoEventArgs serverInfo = new ServerInfoEventArgs { serverName = _serverName };
            SendData(ProtocolPacketType.ServerInfoPacket, serverInfo);
        }

        private void ConnectionLayer_ClientDisconnected(object sender)
        {
            ClientDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void ConnectionLayer_ConnectionLost(object sender)
        {
            ConnectionLost?.Invoke(this, EventArgs.Empty);
        }

        private void ConnectionLayer_PacketReceived(object sender, ConnectionLayerShared.PacketReceivedEventArgs e)
        {
            MemoryStream m = new MemoryStream(e.data as byte[]);
            BinaryFormatter b = new BinaryFormatter();
            ProtocolPacket packet = b.Deserialize(m) as ProtocolPacket;

            switch (packet.packetType)
            {
                case ProtocolPacketType.ProtocolInfoPacket:
                    break;
                case ProtocolPacketType.ClientInfoPacket:
                    {
                        ClientInfoEventArgs args = packet.serializedData as ClientInfoEventArgs;
                        ClientConnected?.Invoke(this, args);
                        break;
                    }
                case ProtocolPacketType.SoundSettingsPacket:
                    {
                        SettingsDataEventArgs args = packet.serializedData as SettingsDataEventArgs;
                        SettingsReceived?.Invoke(this, args);
                        break;
                    }
                case ProtocolPacketType.VoiceWindowPacket:
                    {
                        VoiceWindowDataEventArgs args = packet.serializedData as VoiceWindowDataEventArgs;
                        VoiceWindowReceived?.Invoke(this, args);
                        break;
                    }
                case ProtocolPacketType.ChatMessagePacket:
                    {
                        ClientChatMessageEventArgs args = packet.serializedData as ClientChatMessageEventArgs;
                        ChatMessageReceived?.Invoke(this, args);
                        break;
                    }
                default:
                    throw new Exception("Unknown protocol message");
            }
        }

        public bool IsClientConnected()
        {
            return connectionLayer.IsClientConnected();
        }

        public bool IsListening()
        {
            return connectionLayer.IsListening();
        }

        public ServerProtocol(string serverName)
        {
            _serverName = serverName;
        }

        public bool Bind()
        {
            if (IsListening() || IsClientConnected())
                throw new InvalidOperationException("Already bound");

            bool bindingStatus = connectionLayer.Bind(ProtocolConstants.protocolPort);
            if (!bindingStatus)
                throw new ApplicationException("Unable to bind connection layer");

            connectionLayer.ClientConnected += ConnectionLayer_ClientConnected;
            connectionLayer.ClientDisconnected += ConnectionLayer_ClientDisconnected;
            connectionLayer.PacketReceived += ConnectionLayer_PacketReceived;
            connectionLayer.ConnectionLost += ConnectionLayer_ConnectionLost;

            return true;
        }

        public void Stop()
        {
            connectionLayer.Stop();
        }

        public bool SendSensorsData(double temperatureValue, double skinResistanceValue, double motionValue, double pulseValue)
        {
            SensorsDataEventArgs data = new SensorsDataEventArgs
            {
                motionValue = motionValue,
                skinResistanceValue = skinResistanceValue,
                pulseValue = pulseValue,
                temperatureValue = temperatureValue
            };

            return SendData(ProtocolPacketType.SensorsDataPacket, data);
        }

        public bool SendVoiceWindow(int samplingRate, int bytesPerSample, byte[] voiceData)
        {
            if (voiceData == null)
                return false;

            VoiceWindowDataEventArgs data = new VoiceWindowDataEventArgs(samplingRate, bytesPerSample, voiceData);
            return SendData(ProtocolPacketType.VoiceWindowPacket, data);
        }

        public bool SendChatMessage(string message)
        {
            if (message == null)
                return false;

            ClientChatMessageEventArgs data = new ClientChatMessageEventArgs() { message = message };
            return SendData(ProtocolPacketType.ChatMessagePacket, data);
        }
    }
}
