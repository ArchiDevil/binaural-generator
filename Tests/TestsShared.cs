using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkLayer.Protocol;

namespace Tests
{
    static class TestShared
    {
        public static List<Packet> ParsePackets(byte[] data)
        {
            List<Packet> packets = new List<Packet>();
            if (data == null || data.Length < 5)
                return packets;

            while (true)
            {
                if (data.Length < 5)
                    break;

                PacketType type = (PacketType)data[0];
                int packetSize = BitConverter.ToInt32(data, 1);
                if (packetSize <= 0)
                    break;

                packets.Add(new Packet(type, data.Skip(sizeof(PacketType) + sizeof(int)).Take(packetSize).ToArray()));
                data = data.Skip(sizeof(PacketType) + sizeof(int) + packetSize).ToArray();
            }

            return packets;
        }
    }
}
