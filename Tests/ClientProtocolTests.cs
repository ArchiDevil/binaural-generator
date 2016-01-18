using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;
using NetworkLayer.Protocol;

namespace Tests
{
    [TestClass]
    public sealed class ClientProtocolTests : IDisposable
    {
        ClientProtocol protocol = null;
        InternetServerConnectionInterface server = null;

        ushort protocolPort = ProtocolShared.protocolPort;
        int waitingTimeout = 5000;
        string clientName = "MyName";

        [TestInitialize]
        public void Initialize()
        {
            protocol = new ClientProtocol(clientName);
            server = new InternetServerConnectionInterface();
        }

        [TestCleanup]
        public void Cleanup()
        {
            server.Shutdown();
            server = null;

            protocol.Disconnect();
            protocol = null;
        }

        [TestMethod]
        public void CanProtocolConnect()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
        }

        [TestMethod]
        public void CanProtocolConnectTwice()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            Assert.IsTrue(protocol.Connect("localhost"));
        }

        [TestMethod]
        public void CanProtocolDisconnect()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            protocol.Disconnect();
            Thread.Sleep(10);
            Assert.AreEqual(0, server.Send(new byte[10]));
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
            Assert.IsTrue(server.StartListening(protocolPort));
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
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            byte[] buffer = new byte[1024];
            int count = server.Receive(buffer, 5000);
            Assert.IsTrue(count > 0);
            List<Packet> packets = TestShared.ParsePackets(buffer);

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(PacketType.ClientInfoMessage, packets.First().type);

            BinaryFormatter b = new BinaryFormatter();
            MemoryStream m = new MemoryStream(packets.First().data);
            ClientInfoEventArgs args = (ClientInfoEventArgs)b.Deserialize(m);
            Assert.AreEqual(clientName, args.clientName);
        }

        [TestMethod]
        public void ClientSendsSignalSettings()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            int channelsCount = 2;
            ChannelDescription[] channelDesc = new ChannelDescription[channelsCount];
            for (int i = 0; i < channelsCount; ++i)
            {
                channelDesc[i] = new ChannelDescription(440.0, 10.0, 1.0, true);
            }
            NoiseDescription noiseDesc = new NoiseDescription(10.0, 1.0);
            Assert.IsTrue(protocol.SendSignalSettings(channelDesc, noiseDesc));

            byte[] buffer = new byte[16384];
            int totalCount = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte[] tmp = new byte[16384];
                int count = server.Receive(tmp, 100);
                if (count == 0)
                    break;
                tmp = tmp.Take(count).ToArray();
                tmp.CopyTo(buffer, totalCount);
                totalCount += count;

                // to ensure data receiving
                Thread.Sleep(100);
            }

            Assert.IsTrue(totalCount > 0);

            List<Packet> packets = TestShared.ParsePackets(buffer);
            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(PacketType.ClientInfoMessage, packets[0].type);
            Assert.AreEqual(PacketType.SettingsMessage, packets[1].type);

            BinaryFormatter b = new BinaryFormatter();
            MemoryStream m = new MemoryStream(packets[1].data);
            SettingsDataEventArgs args = (SettingsDataEventArgs)b.Deserialize(m);
            Assert.AreEqual(args.channels.Length, channelDesc.Length);
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
        public void ClientSendSignalSettingsFailed()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            ChannelDescription[] desc = new ChannelDescription[0];
            Assert.IsFalse(protocol.SendSignalSettings(null, new NoiseDescription()));
            Assert.IsFalse(protocol.SendSignalSettings(desc, new NoiseDescription()));
        }

        [TestMethod]
        public void ClientSendsVoiceWindow()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            byte[] voice = new byte[100];
            Assert.IsTrue(protocol.SendVoiceWindow(voice));

            byte[] buffer = new byte[4096];
            int totalCount = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte[] tmp = new byte[1024];
                int count = server.Receive(tmp, 100);
                if (count == 0)
                    break;
                tmp = tmp.Take(count).ToArray();
                tmp.CopyTo(buffer, totalCount);
                totalCount += count;

                // to ensure data receiving
                Thread.Sleep(100);
            }

            Assert.IsTrue(totalCount > 0);

            List<Packet> packets = TestShared.ParsePackets(buffer);
            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(PacketType.ClientInfoMessage, packets[0].type);
            Assert.AreEqual(PacketType.VoiceMessage, packets[1].type);

            BinaryFormatter b = new BinaryFormatter();
            MemoryStream m = new MemoryStream(packets[1].data);
            VoiceWindowDataEventArgs args = (VoiceWindowDataEventArgs)b.Deserialize(m);
            Assert.AreEqual(voice.Length, args.data.Length);
        }

        [TestMethod]
        public void ClientSendVoiceWindowFailed()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            byte[] b = new byte[0];
            Assert.IsFalse(protocol.SendVoiceWindow(b));
            Assert.IsFalse(protocol.SendVoiceWindow(null));
        }

        [TestMethod]
        public void ClientSendsChatMessage()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            string message = "hello";
            Assert.IsTrue(protocol.SendChatMessage(message));

            byte[] buffer = new byte[4096];
            int totalCount = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte[] tmp = new byte[1024];
                int count = server.Receive(tmp, 100);
                if (count == 0)
                    break;
                tmp = tmp.Take(count).ToArray();
                tmp.CopyTo(buffer, totalCount);
                totalCount += count;

                // to ensure data receiving
                Thread.Sleep(100);
            }

            Assert.IsTrue(totalCount > 0);

            List<Packet> packets = TestShared.ParsePackets(buffer);
            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(PacketType.ClientInfoMessage, packets[0].type);
            Assert.AreEqual(PacketType.ChatMessage, packets[1].type);

            BinaryFormatter b = new BinaryFormatter();
            MemoryStream m = new MemoryStream(packets[1].data);
            ClientChatMessageEventArgs args = (ClientChatMessageEventArgs)b.Deserialize(m);
            Assert.AreEqual(message, args.message);
        }

        [TestMethod]
        public void ClientSendChatMessageFailed()
        {
            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            Assert.IsFalse(protocol.SendChatMessage(null));
            Assert.IsFalse(protocol.SendChatMessage(""));
        }

        [TestMethod]
        public void ClientReceivesSensorsMessage()
        {
            ManualResetEvent messageReceived = new ManualResetEvent(false);
            SensorsDataEventArgs args = null;
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();

            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            protocol.SensorsReceive += (s, e) => { messageReceived.Set(); args = e; };

            // create chat message packet
            SensorsDataEventArgs sentArgs = new SensorsDataEventArgs { motionValue = 100500.0, pulseValue = 60.0, skinResistanceValue = 1000.0, temperatureValue = 36.6 };
            b.Serialize(m, sentArgs);

            Packet p = new Packet(PacketType.SensorsMessage, m.GetBuffer());
            Assert.IsTrue(server.Send(p.SerializedData) > 0);
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
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();

            byte[] voiceData = new byte[44100];
            for (int i = 0; i < voiceData.Length; ++i)
                voiceData[i] = (byte)i;

            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            protocol.VoiceWindowReceive += (s, e) => { messageReceived.Set(); args = e; };

            // create chat message packet
            VoiceWindowDataEventArgs sentArgs = new VoiceWindowDataEventArgs { data = voiceData };
            b.Serialize(m, sentArgs);

            Packet p = new Packet(PacketType.VoiceMessage, m.GetBuffer());
            Assert.IsTrue(server.Send(p.SerializedData) > 0);
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));
            for (int i = 0; i < args.data.Length; ++i)
                Assert.AreEqual(sentArgs.data[i], args.data[i]);
        }

        [TestMethod]
        public void ClientReceivesChatMessage()
        {
            ManualResetEvent messageReceived = new ManualResetEvent(false);
            ClientChatMessageEventArgs args = null;
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            string chatMessage = "Hello";

            Assert.IsTrue(server.StartListening(protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            protocol.ChatMessageReceive += (s, e) => { messageReceived.Set(); args = e; };

            // create chat message packet
            ClientChatMessageEventArgs sentArgs = new ClientChatMessageEventArgs { message = chatMessage };
            b.Serialize(m, sentArgs);

            Packet p = new Packet(PacketType.ChatMessage, m.GetBuffer());
            Assert.IsTrue(server.Send(p.SerializedData) > 0);
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));
            Assert.AreEqual(sentArgs.message, args.message);
        }

        public void Dispose()
        {
            if (protocol != null)
                protocol.Dispose();

            if (server != null)
                server.Dispose();
        }
    }
}
