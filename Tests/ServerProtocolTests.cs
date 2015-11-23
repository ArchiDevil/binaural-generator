using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;
using NetworkLayer.Protocol;

namespace Tests
{
    [TestClass]
    public sealed class ServerProtocolTests : IDisposable
    {
        ServerProtocol protocol = null;
        InternetClientConnectionInterface client = null;

        int protocolPort = ProtocolShared.protocolPort;
        int waitingTimeout = 5000;
        string serverName = "MyName";

        byte[] CreateInfoPacket()
        {
            string message = "Test client";
            byte[] msg = new byte[256];
            msg[0] = 2; // client info packet type
            BitConverter.GetBytes(message.Length).CopyTo(msg, 1);
            Encoding.UTF8.GetBytes(message).CopyTo(msg, 5);
            return msg;
        }

        [TestInitialize]
        public void Initialize()
        {
            protocol = new ServerProtocol(serverName);
            client = new InternetClientConnectionInterface();
        }

        [TestCleanup]
        public void Cleanup()
        {
            client.Disconnect();
            client = null;

            protocol.Stop();
            protocol = null;
        }

        [TestMethod]
        public void CanProtocolBind()
        {
            Assert.IsTrue(protocol.Bind());
            Assert.IsTrue(client.Connect("localhost", protocolPort));
        }

        [TestMethod]
        public void CanProtocolStop()
        {
            Assert.IsTrue(protocol.Bind());
            Assert.IsTrue(client.Connect("localhost", protocolPort));
            client.Disconnect();
            protocol.Stop();
            Assert.IsFalse(client.Connect("localhost", protocolPort));
        }

        [TestMethod]
        public void CanProtocolRestart()
        {
            for (int i = 0; i < 5; ++i)
            {
                Assert.IsTrue(protocol.Bind());
                Assert.IsTrue(client.Connect("localhost", protocolPort));
                client.Disconnect();
                protocol.Stop();
            }
        }

        [TestMethod]
        public void ProtocolEventOnClientConnected()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));

            // client info packet
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(ev.WaitOne(waitingTimeout));
        }

        [TestMethod]
        public void ProtocolEventOnClientConnectedCheckInfo()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            ClientInfoEventArgs receivedInfo = null;

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => { ev.Set(); receivedInfo = e; };
            Assert.IsTrue(client.Connect("localhost", protocolPort));

            // client info packet
            string message = "Test client";
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.AreNotEqual(null, receivedInfo);
            Assert.AreEqual(message, receivedInfo.clientName);
        }

        [TestMethod]
        public void ProtocolSendsChatMessage()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            string messageToSend = "Hello";

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendChatMessage(messageToSend));

            byte[] msg = new byte[4096];
            int totalCount = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte[] tmp = new byte[1024];
                int count = client.Receive(tmp, 100);
                if (count == 0)
                    break;
                tmp = tmp.Take(count).ToArray();
                tmp.CopyTo(msg, totalCount);
                totalCount += count;

                // to ensure data receiving
                Thread.Sleep(100);
            }
            msg = msg.Take(totalCount).ToArray();
            Assert.IsTrue(totalCount > 0);

            List<Packet> packets = TestShared.ParsePackets(msg);
            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(PacketType.ProtocolInfoMessage, packets[0].type);
            Assert.AreEqual(PacketType.ServerInfoMessage, packets[1].type);
            Assert.AreEqual(PacketType.ChatMessage, packets[2].type);

            BinaryFormatter f = new BinaryFormatter();
            MemoryStream m = new MemoryStream(packets[2].data);
            ClientChatMessageEventArgs chatMessage = (ClientChatMessageEventArgs)f.Deserialize(m);
            Assert.AreEqual(messageToSend, chatMessage.message);
        }

        [TestMethod]
        public void ProtocolSendChatMessageFailed()
        {
            Assert.IsTrue(protocol.Bind());
            Assert.IsFalse(protocol.SendChatMessage(null));
            Assert.IsFalse(protocol.SendChatMessage("Hello"));
            Assert.IsFalse(protocol.SendChatMessage(""));
        }

        [TestMethod]
        public void ProtocolSendsSensorsData()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            SensorsDataEventArgs sensorsData = new SensorsDataEventArgs
            {
                motionValue = 1.0,
                pulseValue = 64.0,
                skinResistanceValue = 100500.0,
                temperatureValue = 36.6
            };

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);

            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendSensorsData(sensorsData.temperatureValue,
                                                   sensorsData.skinResistanceValue,
                                                   sensorsData.motionValue,
                                                   sensorsData.pulseValue));

            byte[] msg = new byte[4096];
            int totalCount = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte[] tmp = new byte[1024];
                int count = client.Receive(tmp, 100);
                if (count == 0)
                    break;
                tmp = tmp.Take(count).ToArray();
                tmp.CopyTo(msg, totalCount);
                totalCount += count;

                // to ensure data receiving
                Thread.Sleep(100);
            }
            msg = msg.Take(totalCount).ToArray();
            Assert.IsTrue(totalCount > 0);

            List<Packet> packets = TestShared.ParsePackets(msg);
            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(PacketType.ProtocolInfoMessage, packets[0].type);
            Assert.AreEqual(PacketType.ServerInfoMessage, packets[1].type);
            Assert.AreEqual(PacketType.SensorsMessage, packets[2].type);

            BinaryFormatter f = new BinaryFormatter();
            MemoryStream m = new MemoryStream(packets[2].data);
            SensorsDataEventArgs receivedData = (SensorsDataEventArgs)f.Deserialize(m);
            double eps = 0.0001;
            Assert.AreEqual(sensorsData.motionValue, receivedData.motionValue, eps);
            Assert.AreEqual(sensorsData.pulseValue, receivedData.pulseValue, eps);
            Assert.AreEqual(sensorsData.skinResistanceValue, receivedData.skinResistanceValue, eps);
            Assert.AreEqual(sensorsData.temperatureValue, receivedData.temperatureValue, eps);
        }

        [TestMethod]
        public void ProtocolSendSensorsDataFailed()
        {
            Assert.IsTrue(protocol.Bind());
            Assert.IsFalse(protocol.SendSensorsData(motionValue: 1.0, pulseValue: 64.0, skinResistanceValue: 100500.0, temperatureValue: 36.6));
        }

        [TestMethod]
        public void ProtocolSendsVoiceWindow()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);

            byte[] voiceData = new byte[44100];
            for (int i = 0; i < voiceData.Length; ++i)
                voiceData[i] = (byte)i;

            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendVoiceWindow(voiceData));

            byte[] msg = new byte[441000];
            int totalCount = 0;
            for (int i = 0; i < 10; ++i)
            {
                byte[] tmp = new byte[44100];
                int count = client.Receive(tmp, 100);
                if (count == 0)
                    break;
                tmp = tmp.Take(count).ToArray();
                tmp.CopyTo(msg, totalCount);
                totalCount += count;

                // to ensure data receiving
                Thread.Sleep(100);
            }
            msg = msg.Take(totalCount).ToArray();
            Assert.IsTrue(totalCount > 0);

            List<Packet> packets = TestShared.ParsePackets(msg);
            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(PacketType.ProtocolInfoMessage, packets[0].type);
            Assert.AreEqual(PacketType.ServerInfoMessage, packets[1].type);
            Assert.AreEqual(PacketType.VoiceMessage, packets[2].type);

            BinaryFormatter f = new BinaryFormatter();
            MemoryStream m = new MemoryStream(packets[2].data);
            VoiceWindowDataEventArgs receivedData = (VoiceWindowDataEventArgs)f.Deserialize(m);
            for (int i = 0; i < receivedData.data.Length; ++i)
                Assert.AreEqual(voiceData[i], receivedData.data[i]);
        }

        [TestMethod]
        public void ProtocolSendVoiceWindowFailed()
        {
            Assert.IsTrue(protocol.Bind());
            byte[] voiceData = new byte[44100];
            for (int i = 0; i < voiceData.Length; ++i)
                voiceData[i] = (byte)i;

            Assert.IsFalse(protocol.SendVoiceWindow(null));
            Assert.IsFalse(protocol.SendVoiceWindow(voiceData));
        }

        [TestMethod]
        public void ProtocolReceivesChatMessage()
        {
            ManualResetEvent connected = new ManualResetEvent(false);
            ManualResetEvent messageReceived = new ManualResetEvent(false);

            ClientChatMessageEventArgs args = null;

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => connected.Set();
            protocol.ChatMessageReceive += (s, e) => { messageReceived.Set(); args = e; };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(connected.WaitOne(waitingTimeout));

            // create chat message packet
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            string message = "Hello!";
            ClientChatMessageEventArgs sentArgs = new ClientChatMessageEventArgs { message = message };
            b.Serialize(m, sentArgs);

            Packet packet = new Packet(PacketType.ChatMessage, m.GetBuffer());
            Assert.IsTrue(client.Send(packet.SerializedData) > 0);
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));
            Assert.AreEqual(message, args.message);
        }

        [TestMethod]
        public void ProtocolReceivesSettingsEvent()
        {
            ManualResetEvent connected = new ManualResetEvent(false);
            ManualResetEvent messageReceived = new ManualResetEvent(false);

            SettingsDataEventArgs args = null;

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => connected.Set();
            protocol.SettingsReceive += (s, e) => { messageReceived.Set(); args = e; };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(connected.WaitOne(waitingTimeout));

            // create packet
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            int channelsCount = 2;
            ChannelDescription[] channelDesc = new ChannelDescription[2];
            for (int i = 0; i < channelsCount; ++i)
                channelDesc[i] = new ChannelDescription(10.0, 20.0, 30.0);

            NoiseDescription noiseDesc = new NoiseDescription(10.0, 20.0);
            SettingsDataEventArgs sentArgs = new SettingsDataEventArgs { channels = channelDesc, noise = noiseDesc };
            b.Serialize(m, sentArgs);

            Packet packet = new Packet(PacketType.SettingsMessage, m.GetBuffer());
            Assert.IsTrue(client.Send(packet.SerializedData) > 0);
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));

            Assert.AreEqual(channelDesc.Length, args.channels.Length);
            for(int i = 0; i < channelsCount; ++i)
            {
                Assert.AreEqual(channelDesc[i].carrierFrequency, args.channels[i].carrierFrequency, 0.0001);
                Assert.AreEqual(channelDesc[i].differenceFrequency, args.channels[i].differenceFrequency, 0.0001);
                Assert.AreEqual(channelDesc[i].volume, args.channels[i].volume, 0.0001);
            }
            Assert.AreEqual(noiseDesc.smoothness, args.noise.smoothness, 0.0001);
            Assert.AreEqual(noiseDesc.volume, args.noise.volume, 0.0001);
        }

        [TestMethod]
        public void ProtocolReceivesVoiceWindow()
        {
            ManualResetEvent connected = new ManualResetEvent(false);
            ManualResetEvent voiceReceived = new ManualResetEvent(false);
            VoiceWindowDataEventArgs args = null;

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => connected.Set();
            protocol.VoiceWindowReceive += (s, e) => { voiceReceived.Set(); args = e; };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(connected.WaitOne(waitingTimeout));

            byte[] voiceData = new byte[44100];
            for (int i = 0; i < voiceData.Length; ++i)
                voiceData[i] = (byte)i;

            // create chat message packet
            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            VoiceWindowDataEventArgs sentArgs = new VoiceWindowDataEventArgs { data = voiceData };
            b.Serialize(m, sentArgs);

            Packet packet = new Packet(PacketType.VoiceMessage, m.GetBuffer());
            Assert.IsTrue(client.Send(packet.SerializedData) > 0);
            Assert.IsTrue(voiceReceived.WaitOne(waitingTimeout));
            Assert.IsTrue(Enumerable.SequenceEqual(voiceData, args.data));
        }

        [TestMethod]
        public void ProtocolSendsNameAfterConnectionComplete()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));

            // client info packet
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(ev.WaitOne(waitingTimeout));

            byte[] msg = new byte[4096];
            int totalCount = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte[] tmp = new byte[1024];
                int count = client.Receive(tmp, 100);
                if (count == 0)
                    break;
                tmp = tmp.Take(count).ToArray();
                tmp.CopyTo(msg, totalCount);
                totalCount += count;

                // to ensure data receiving
                Thread.Sleep(100);
            }
            msg = msg.Take(totalCount).ToArray();
            Assert.IsTrue(totalCount > 0);

            List<Packet> packets = TestShared.ParsePackets(msg);
            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(PacketType.ProtocolInfoMessage, packets[0].type);
            Assert.AreEqual(PacketType.ServerInfoMessage, packets[1].type);

            BinaryFormatter f = new BinaryFormatter();
            MemoryStream m = new MemoryStream(packets[1].data);
            ServerInfoEventArgs serverInfo = (ServerInfoEventArgs)f.Deserialize(m);
            Assert.AreEqual(serverName, serverInfo.serverName);
        }

        public void Dispose()
        {
            if (protocol != null)
                protocol.Dispose();

            if (client != null)
                client.Dispose();
        }
    }
}
