using System;

namespace NetworkLayer.ProtocolShared
{
    internal static class ProtocolConstants
    {
        internal const int protocolPort = 31012;
    }

    enum ProtocolPacketType : byte
    {
        Unknown,
        ProtocolInfoPacket,
        ClientInfoPacket,
        ServerInfoPacket,
        SoundSettingsPacket,
        SensorsDataPacket,
        VoiceWindowPacket,
        ChatMessagePacket,
    }

    /// <summary>
    /// This class encapsulates protocol packets to send over connection layer
    /// </summary>
    [Serializable]
    internal class ProtocolPacket
    {
        public ProtocolPacketType packetType;
        public object serializedData;
    }

    /// <summary>
    /// This class contains all information about currently used protocol.
    /// If something is changed, you should update protocol version
    /// to break compatibility with previous version
    /// </summary>
    [Serializable]
    internal class ProtocolInfo
    {
        public int protocolVersion = 1;
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
        public bool enabled;
        public double carrierFrequency;
        public double differenceFrequency;
        public double volume;

        public ChannelDescription(double carrierFrequency, double differenceFrequency, double volume, bool enabled)
        {
            this.enabled = enabled;
            this.carrierFrequency = carrierFrequency;
            this.differenceFrequency = differenceFrequency;
            this.volume = volume;
        }
    }

    [Serializable]
    public struct NoiseDescription
    {
        public bool enabled;
        public double smoothness;
        public double volume;

        public NoiseDescription(bool enabled, double smoothness, double volume)
        {
            this.enabled = enabled;
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
