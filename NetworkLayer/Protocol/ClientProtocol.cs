using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NetworkLayer.Client;
using NetworkLayer.ProtocolShared;

namespace NetworkLayer
{
    public sealed class ClientProtocol
    {
        public delegate void SensorsReceiveHandler(object sender, SensorsDataEventArgs e);
        public delegate void VoiceWindowReceiveHandler(object sender, VoiceWindowDataEventArgs e);
        public delegate void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e);

        IClientConnectionLayer connectionLayer = new ClientNetworkConnectionLayer();

        string _clientName = string.Empty;

        private bool SendPacket(ProtocolPacketType type, object data)
        {
            if (!IsConnected() || type == ProtocolPacketType.Unknown || data == null)
                return false;

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

        public ClientProtocol(string clientName)
        {
            _clientName = clientName;
        }

        public bool Connect(string address)
        {
            if (IsConnected())
            {
                throw new InvalidOperationException("Already connected");
            }

            if (!connectionLayer.Connect(address, ProtocolConstants.protocolPort))
                return false;

            connectionLayer.PacketReceived += ConnectionLayer_PacketReceived;

            ProtocolInfo protocolInfo = new ProtocolInfo();
            if (!SendPacket(ProtocolPacketType.ProtocolInfoPacket, protocolInfo))
            {
                Disconnect();
                return false;
            }

            ClientInfoEventArgs data = new ClientInfoEventArgs { clientName = _clientName };
            if (!SendPacket(ProtocolPacketType.ClientInfoPacket, data))
            {
                Disconnect();
                return false;
            }

            return true;
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
                case ProtocolPacketType.ServerInfoPacket:
                    break;
                case ProtocolPacketType.SensorsDataPacket:
                    {
                        SensorsDataEventArgs args = packet.serializedData as SensorsDataEventArgs;
                        SensorsReceived(this, args);
                        break;
                    }
                case ProtocolPacketType.VoiceWindowPacket:
                    {
                        VoiceWindowDataEventArgs args = packet.serializedData as VoiceWindowDataEventArgs;
                        VoiceWindowReceived(this, args);
                        break;
                    }
                case ProtocolPacketType.ChatMessagePacket:
                    {
                        ClientChatMessageEventArgs args = packet.serializedData as ClientChatMessageEventArgs;
                        ChatMessageReceived(this, args);
                        break;
                    }
                default:
                    throw new Exception("Unknown protocol message");
            }
        }

        public bool IsConnected()
        {
            return connectionLayer.IsConnected();
        }

        public void Disconnect()
        {
            connectionLayer.Disconnect();
        }

        public bool SendSignalSettings(ChannelDescription[] channels, NoiseDescription noise)
        {
            if (channels == null || channels.Length == 0)
                return false;

            SettingsDataEventArgs data = new SettingsDataEventArgs { channels = channels, noise = noise };
            return SendPacket(ProtocolPacketType.SoundSettingsPacket, data);
        }

        public bool SendVoiceWindow(byte[] voiceData)
        {
            if (voiceData == null || voiceData.Length == 0)
                return false;

            VoiceWindowDataEventArgs data = new VoiceWindowDataEventArgs { data = voiceData };
            return SendPacket(ProtocolPacketType.VoiceWindowPacket, data);
        }

        public bool SendChatMessage(string message)
        {
            if (message == null || message.Length == 0)
                return false;

            ClientChatMessageEventArgs data = new ClientChatMessageEventArgs { message = message };
            return SendPacket(ProtocolPacketType.ChatMessagePacket, data);
        }

        public event SensorsReceiveHandler SensorsReceived = delegate { };
        public event VoiceWindowReceiveHandler VoiceWindowReceived = delegate { };
        public event ChatMessageReceiveHandler ChatMessageReceived = delegate { };
    }
}
