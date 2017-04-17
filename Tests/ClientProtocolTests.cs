using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;
using NetworkLayer.Server;
using NetworkLayer.ProtocolShared;

namespace Tests
{
    [TestClass]
    public sealed class ClientProtocolTests : IDisposable
    {
        ClientProtocol protocol = null;
        ServerNetworkConnectionLayer server = null;

        ushort protocolPort = ProtocolConstants.protocolPort;
        int waitingTimeout = 5000;
        string clientName = "MyName";

        byte[] CreateInfoPacket()
        {
            ProtocolPacket protocolPacket = new ProtocolPacket()
            {
                packetType = ProtocolPacketType.ServerInfoPacket,
                serializedData = new ServerInfoEventArgs()
                {
                    serverName = "Test server"
                }
            };

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, protocolPacket);
            return m.GetBuffer();
        }

        [TestInitialize]
        public void Initialize()
        {
            protocol = new ClientProtocol(clientName);
            server = new ServerNetworkConnectionLayer();
        }

        [TestCleanup]
        public void Cleanup()
        {
            protocol.Disconnect();
            protocol = null;

            server.Stop();
            server = null;
        }

        [TestMethod]
        public void CanProtocolConnect()
        {
            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
        }

        [TestMethod]
        public void CanProtocolDisconnect()
        {
            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            protocol.Disconnect();
            Thread.Sleep(200);
            Assert.IsFalse(server.IsClientConnected());
            Assert.IsFalse(server.SendData(new byte[10]));
        }

        [TestMethod]
        public void ProtocolDisconnectFailed()
        {
            try
            {
                protocol.Disconnect();
                protocol.Disconnect();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CanProtocolReconnect()
        {
            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            protocol.Disconnect();
            Assert.IsTrue(protocol.Connect("localhost"));
        }

        [TestMethod]
        public void ProtocolSendFailed()
        {
            Assert.IsFalse(protocol.SendChatMessage("Test message"));
        }

        [TestMethod]
        public void ClientSendsInfoAfterConnect()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            List<ProtocolPacket> packets = new List<ProtocolPacket>();

            server.PacketReceived += (x, y) =>
            {
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream m = new MemoryStream(y.data as byte[]);
                ProtocolPacket packet = f.Deserialize(m) as ProtocolPacket;
                packets.Add(packet);
            };

            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            Thread.Sleep(200);

            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(ProtocolPacketType.ProtocolInfoPacket, packets[0].packetType);
            Assert.AreEqual(ProtocolPacketType.ClientInfoPacket, packets[1].packetType);

            ClientInfoEventArgs args = packets[1].serializedData as ClientInfoEventArgs;
            Assert.AreEqual(clientName, args.clientName);
        }

        [TestMethod]
        public void ClientSendsSignalSettings()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            List<ProtocolPacket> packets = new List<ProtocolPacket>();

            server.PacketReceived += (x, y) =>
            {
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream m = new MemoryStream(y.data as byte[]);
                ProtocolPacket packet = f.Deserialize(m) as ProtocolPacket;
                packets.Add(packet);
            };

            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            int channelsCount = 2;
            ChannelDescription[] channelDesc = new ChannelDescription[channelsCount];
            for (int i = 0; i < channelsCount; ++i)
            {
                channelDesc[i] = new ChannelDescription(440.0, 10.0, 1.0, true);
            }
            NoiseDescription noiseDesc = new NoiseDescription(true, 10.0, 1.0);
            Assert.IsTrue(protocol.SendSignalSettings(channelDesc, noiseDesc));
            Thread.Sleep(200);

            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(ProtocolPacketType.ProtocolInfoPacket, packets[0].packetType);
            Assert.AreEqual(ProtocolPacketType.ClientInfoPacket, packets[1].packetType);
            Assert.AreEqual(ProtocolPacketType.SoundSettingsPacket, packets[2].packetType);

            SettingsDataEventArgs args = packets[2].serializedData as SettingsDataEventArgs;
            Assert.AreEqual(args.channels.Length, channelDesc.Length);
            Assert.AreEqual(args.noise.enabled, noiseDesc.enabled);
            Assert.AreEqual(args.noise.smoothness, noiseDesc.smoothness, 0.0001);
            Assert.AreEqual(args.noise.volume, noiseDesc.volume, 0.0001);

            for (int i = 0; i < channelsCount; ++i)
            {
                Assert.AreEqual(args.channels[i].carrierFrequency, channelDesc[i].carrierFrequency, 0.0001);
                Assert.AreEqual(args.channels[i].differenceFrequency, channelDesc[i].differenceFrequency, 0.0001);
                Assert.AreEqual(args.channels[i].volume, channelDesc[i].volume, 0.0001);
                Assert.AreEqual(args.channels[i].enabled, channelDesc[i].enabled);
            }
        }

        [TestMethod]
        public void ClientSendsVoiceWindow()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            List<ProtocolPacket> packets = new List<ProtocolPacket>();

            server.PacketReceived += (x, y) =>
            {
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream m = new MemoryStream(y.data as byte[]);
                ProtocolPacket packet = f.Deserialize(m) as ProtocolPacket;
                packets.Add(packet);
            };

            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            byte[] voice = new byte[100];
            Assert.IsTrue(protocol.SendVoiceWindow(voice));
            Thread.Sleep(200);

            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(ProtocolPacketType.ProtocolInfoPacket, packets[0].packetType);
            Assert.AreEqual(ProtocolPacketType.ClientInfoPacket, packets[1].packetType);
            Assert.AreEqual(ProtocolPacketType.VoiceWindowPacket, packets[2].packetType);

            VoiceWindowDataEventArgs args = packets[2].serializedData as VoiceWindowDataEventArgs;
            Assert.AreEqual(voice.Length, args.data.Length);
        }

        [TestMethod]
        public void ClientSendsChatMessage()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            List<ProtocolPacket> packets = new List<ProtocolPacket>();

            server.PacketReceived += (x, y) =>
            {
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream m = new MemoryStream(y.data as byte[]);
                ProtocolPacket packet = f.Deserialize(m) as ProtocolPacket;
                packets.Add(packet);
            };

            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            string message = "hello";
            Assert.IsTrue(protocol.SendChatMessage(message));
            Thread.Sleep(200);

            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(ProtocolPacketType.ProtocolInfoPacket, packets[0].packetType);
            Assert.AreEqual(ProtocolPacketType.ClientInfoPacket, packets[1].packetType);
            Assert.AreEqual(ProtocolPacketType.ChatMessagePacket, packets[2].packetType);

            ClientChatMessageEventArgs args = packets[2].serializedData as ClientChatMessageEventArgs;
            Assert.AreEqual(message, args.message);
        }

        [TestMethod]
        public void ClientSendSignalSettingsFailed()
        {
            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            ChannelDescription[] desc = new ChannelDescription[0];
            Assert.IsFalse(protocol.SendSignalSettings(null, new NoiseDescription()));
            Assert.IsFalse(protocol.SendSignalSettings(desc, new NoiseDescription()));
        }

        [TestMethod]
        public void ClientSendVoiceWindowFailed()
        {
            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            byte[] b = new byte[0];
            Assert.IsFalse(protocol.SendVoiceWindow(b));
            Assert.IsFalse(protocol.SendVoiceWindow(null));
        }

        [TestMethod]
        public void ClientSendChatMessageFailed()
        {
            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            Assert.IsFalse(protocol.SendChatMessage(null));
            Assert.IsFalse(protocol.SendChatMessage(""));
        }

        [TestMethod]
        public void ClientReceivesSensorsMessage()
        {
            ManualResetEvent messageReceived = new ManualResetEvent(false);
            SensorsDataEventArgs args = null;

            protocol.SensorsReceived += (s, e) => { messageReceived.Set(); args = e; };

            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            server.SendData(CreateInfoPacket());

            // create chat message packet
            SensorsDataEventArgs sentArgs = new SensorsDataEventArgs { motionValue = 100500.0, pulseValue = 60.0, skinResistanceValue = 1000.0, temperatureValue = 36.6 };
            ProtocolPacket p = new ProtocolPacket() { packetType = ProtocolPacketType.SensorsDataPacket, serializedData = sentArgs };

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, p);

            Assert.IsTrue(server.SendData(m.GetBuffer()));
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));
            Assert.AreEqual(sentArgs.motionValue, args.motionValue, 0.0001);
            Assert.AreEqual(sentArgs.pulseValue, args.pulseValue, 0.0001);
            Assert.AreEqual(sentArgs.skinResistanceValue, args.skinResistanceValue, 0.0001);
            Assert.AreEqual(sentArgs.temperatureValue, args.temperatureValue, 0.0001);
        }

        [TestMethod]
        public void ClientReceivesVoiceMessage()
        {
            ManualResetEvent messageReceived = new ManualResetEvent(false);
            VoiceWindowDataEventArgs args = null;

            byte[] voiceData = new byte[44100];
            for (int i = 0; i < voiceData.Length; ++i)
                voiceData[i] = (byte)i;

            protocol.VoiceWindowReceived += (s, e) => { messageReceived.Set(); args = e; };

            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            server.SendData(CreateInfoPacket());

            // create chat message packet
            VoiceWindowDataEventArgs sentArgs = new VoiceWindowDataEventArgs { data = voiceData };
            ProtocolPacket p = new ProtocolPacket() { packetType = ProtocolPacketType.VoiceWindowPacket, serializedData = sentArgs };

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, p);

            Assert.IsTrue(server.SendData(m.GetBuffer()));
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));
            for (int i = 0; i < args.data.Length; ++i)
                Assert.AreEqual(sentArgs.data[i], args.data[i]);
        }

        [TestMethod]
        public void ClientReceivesChatMessage()
        {
            ManualResetEvent messageReceived = new ManualResetEvent(false);
            ClientChatMessageEventArgs args = null;

            string chatMessage = "Hello";
            protocol.ChatMessageReceived += (s, e) => { messageReceived.Set(); args = e; };

            Assert.IsTrue(server.Bind(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            ClientChatMessageEventArgs sentArgs = new ClientChatMessageEventArgs { message = chatMessage };
            ProtocolPacket p = new ProtocolPacket() { packetType = ProtocolPacketType.ChatMessagePacket, serializedData = sentArgs };

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, p);

            Assert.IsTrue(server.SendData(m.GetBuffer()));
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));
            Assert.AreEqual(sentArgs.message, args.message);
        }

        public void Dispose()
        {
            server?.Dispose();
        }
    }
}
