using System;

namespace NetworkLayer.Protocol
{
    internal static class ProtocolShared
    {
        internal const int protocolPort = 31012;
    }

    /// <summary>
    /// This packet type is used as header in protocol.
    /// This is explicitly inherited from byte to make sure bytes count sent.
    /// </summary>
    internal enum PacketType : byte
    {
        Unknown,
        ProtocolInfoMessage,
        ClientInfoMessage,
        ServerInfoMessage,
        ChatMessage,
        VoiceMessage,
        SettingsMessage,
        SensorsMessage,
        // please, add new packet types in the end
    }

    /// <summary>
    /// This class encapsulates serialization logics to send structures over Internet
    /// </summary>
    internal class Packet
    {
        readonly public PacketType type = PacketType.Unknown;
        readonly public byte[] data = null;

        internal byte[] SerializedData
        {
            get
            {
                // packet type header + packet data size + packet data
                int bufferSize = data.Length;
                byte[] packetData = new byte[bufferSize + sizeof(PacketType) + sizeof(int)];

                // packet type header
                BitConverter.GetBytes((byte)type).CopyTo(packetData, 0);

                // packet data size
                BitConverter.GetBytes(bufferSize).CopyTo(packetData, sizeof(PacketType));

                // packet data
                data.CopyTo(packetData, sizeof(PacketType) + sizeof(int));

                return packetData;
            }
        }

        internal Packet(PacketType type, byte[] data)
        {
            this.type = type;
            this.data = data;
        }
    }

    [Serializable]
    internal class ProtocolInfo
    {
        public int protocolVersion = -1;
    }

    /// <summary>
    /// This class contains all information about connected client.
    /// This information needed to identify experimenter
    /// and show this information in subject's UI
    /// </summary>
    [Serializable]
    public class ClientInfoEventArgs : EventArgs
    {
        public string clientName = "";
    }

    /// <summary>
    /// This class contains all information server (subject.
    /// This information needed to identify server and store logs with unique
    /// identificator to classify experiments and subjects.
    /// </summary>
    [Serializable]
    public class ServerInfoEventArgs : EventArgs
    {
        public string serverName = "";
    }

    [Serializable]
    public struct ChannelDescription
    {
        public double carrierFrequency;
        public double differenceFrequency;
        public double volume;
        public bool   enabled;

        public ChannelDescription(double carrierFrequency, double differenceFrequency, double volume, bool enabled)
        {
            this.carrierFrequency = carrierFrequency;
            this.differenceFrequency = differenceFrequency;
            this.volume = volume;
            this.enabled = enabled;
        }
    }

    [Serializable]
    public struct NoiseDescription
    {
        public double smoothness;
        public double volume;

        public NoiseDescription(double smoothness, double volume)
        {
            this.smoothness = smoothness;
            this.volume = volume;
        }
    }

    /// <summary>
    /// This class contains all information about new current settings such
    /// binaural waves set, noise set, additional sounds if needed.
    /// This class is managed by experimenter and applied directly to sound core.
    /// </summary>
    [Serializable]
    public class SettingsDataEventArgs : EventArgs
    {
        public ChannelDescription[] channels = new ChannelDescription[4];
        public NoiseDescription noise = new NoiseDescription();
    }

    /// <summary>
    /// This class contains all sensors data.
    /// </summary>
    [Serializable]
    public class SensorsDataEventArgs : EventArgs
    {
        public double temperatureValue = 0.0;
        public double skinResistanceValue = 0.0;
        public double motionValue = 0.0;
        public double pulseValue = 0.0;
    }

    /// <summary>
    /// This class contains samples from microphone to send it to another side.
    /// </summary>
    [Serializable]
    public class VoiceWindowDataEventArgs : EventArgs
    {
        public int samplingRate = 44100;
        public byte[] data = new byte[44100]; // sampling rate is 44100 Hz
    }

    /// <summary>
    /// This class contains just chat message to communicate with experimenter and subject
    /// </summary>
    [Serializable]
    public class ClientChatMessageEventArgs : EventArgs
    {
        public DateTime sentTime = DateTime.Now;
        public string message = "";
    }
}
