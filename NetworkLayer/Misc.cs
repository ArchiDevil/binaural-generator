using System;

namespace NetworkLayer
{
    /// <summary>
    /// This packet type is used as header in protocol.
    /// This is explicitly inherited from byte to make sure bytes count sent.
    /// </summary>
    internal enum PacketType : byte
    {
        Unknown,
        ProtocolInfoMessage,
        ClientInfoMessage,
        ChatMessage,
        VoiceMessage,
        SettingsMessage,
        SensorsMessage
    }

    /// <summary>
    /// This class encapsulates serialization logics to send structures over Internet
    /// </summary>
    internal class Packet
    {
        PacketType type = PacketType.Unknown;
        byte[] data = null;

        internal byte[] SerializedData
        {
            get
            {
                // packet type header + packet data size + packet data
                int bufferSize = sizeof(PacketType) + sizeof(int) + data.Length;
                byte[] packetData = new byte[bufferSize];

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
    /// This information needed to identify client and store logs with unique
    /// identificator to classify experiments and subjects.
    /// </summary>
    public class ClientInfo
    {
        public string clientName = null;
    }

    /// <summary>
    /// This class contains all information about new current settings such
    /// binaural waves set, noise set, additional sounds if needed.
    /// This class is managed by experimenter and applied directly to sound core.
    /// </summary>
    public class SettingsData
    {
    }

    /// <summary>
    /// This class contains all sensors data.
    /// </summary>
    public class SensorsData
    {
        public double temperatureValue { get; set; } = 0.0;
        public double skinResistanceValue { get; set; } = 0.0;
        public double motionValue { get; set; } = 0.0;
        public double pulseValue { get; set; } = 0.0;
    }

    /// <summary>
    /// This class contains samples from microphone to send it to another side.
    /// </summary>
    public class VoiceWindowData
    {
        public byte[] data = new byte[44100]; // sampling rate is 44100 Hz
    }
}
