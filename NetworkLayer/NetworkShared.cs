using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace NetworkLayer.ConnectionLayerShared
{
    internal class ConnectionLayerConstants
    {
        internal const string handshakeString = "HSHK";
    }

    /// <summary>
    /// This packet type is used as header in protocol.
    /// This is explicitly inherited from byte to make sure bytes count sent.
    /// </summary>
    internal enum PacketType : byte
    {
        Unknown,
        FinishingPacket,
        UserDataPacket
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

    public class PacketReceivedEventArgs : EventArgs
    {
        public object data = null;
    }

    internal static class SocketExtensions
    {
        internal static bool IsConnected(this Socket s)
        {
            try
            {
                return !(s.Poll(1000, SelectMode.SelectRead) && s.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }

    internal static class SharedFunctions
    {
        internal delegate void PacketReceivedDelegate(PacketReceivedEventArgs e);

        internal static void ExtractPackets(ref List<byte> receivedBuffer, out bool disconnection, PacketReceivedDelegate receivedDelegate)
        {
            const int headerSize = sizeof(PacketType) + sizeof(int);
            disconnection = false;

            while (receivedBuffer.Count > 0)
            {
                byte[] protocolHeader = receivedBuffer.Take(headerSize).ToArray();
                PacketType type = (PacketType)protocolHeader[0];
                int packetDataSize = BitConverter.ToInt32(protocolHeader, sizeof(PacketType));

                if (receivedBuffer.Count < headerSize + packetDataSize)
                    break;

                byte[] packetData = receivedBuffer.Skip(headerSize).Take(packetDataSize).ToArray();
                receivedBuffer.RemoveRange(0, headerSize + packetDataSize);

                switch (type)
                {
                    case PacketType.UserDataPacket:
                        receivedDelegate(new PacketReceivedEventArgs { data = packetData });
                        break;
                    case PacketType.FinishingPacket:
                        disconnection = true;
                        break;
                    default:
                        throw new Exception("Unknown connection layer message");
                }
            }
        }
    }
}
