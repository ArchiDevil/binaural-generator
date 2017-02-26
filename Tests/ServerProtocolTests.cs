using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;
using NetworkLayer.Client;
using NetworkLayer.ProtocolShared;

namespace Tests
{
    [TestClass]
    public sealed class ServerProtocolTests : IDisposable
    {
        ServerProtocol protocol = null;
        ClientNetworkConnectionLayer client = null;

        ushort protocolPort = ProtocolConstants.protocolPort;
        int waitingTimeout = 5000;
        string serverName = "MyName";

        byte[] CreateInfoPacket()
        {
            ProtocolPacket protocolPacket = new ProtocolPacket()
            {
                packetType = ProtocolPacketType.ClientInfoPacket,
                serializedData = new ClientInfoEventArgs()
                {
                    clientName = "Test client"
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
            protocol = new ServerProtocol(serverName);
            client = new ClientNetworkConnectionLayer();
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
        public void CanClientConnectTwice()
        {
            Assert.IsTrue(protocol.Bind());
            Assert.IsTrue(client.Connect("localhost", protocolPort));
            client.Disconnect();
            Assert.IsTrue(client.Connect("localhost", protocolPort));
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
            Assert.IsTrue(client.SendData(CreateInfoPacket()));
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
            Assert.IsTrue(client.SendData(CreateInfoPacket()));
            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.AreNotEqual(null, receivedInfo);
            string message = "Test client";
            Assert.AreEqual(message, receivedInfo.clientName);
        }

        [TestMethod]
        public void ProtocolSendsChatMessage()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            string messageToSend = "Hello";
            List<ProtocolPacket> packets = new List<ProtocolPacket>();

            Assert.IsTrue(protocol.Bind());

            protocol.ClientConnected += (s, e) => ev.Set();
            client.PacketReceived += (s, e) =>
            {
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream m = new MemoryStream(e.data as byte[]);
                ProtocolPacket packet = f.Deserialize(m) as ProtocolPacket;
                packets.Add(packet);
            };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.SendData(CreateInfoPacket()));

            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendChatMessage(messageToSend));
            Thread.Sleep(200);

            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(ProtocolPacketType.ProtocolInfoPacket, packets[0].packetType);
            Assert.AreEqual(ProtocolPacketType.ServerInfoPacket, packets[1].packetType);
            Assert.AreEqual(ProtocolPacketType.ChatMessagePacket, packets[2].packetType);

            ClientChatMessageEventArgs chatMessage = packets[2].serializedData as ClientChatMessageEventArgs;
            Assert.AreEqual(messageToSend, chatMessage.message);
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
            List<ProtocolPacket> packets = new List<ProtocolPacket>();

            Assert.IsTrue(protocol.Bind());

            protocol.ClientConnected += (s, e) => ev.Set();
            client.PacketReceived += (s, e) =>
            {
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream m = new MemoryStream(e.data as byte[]);
                ProtocolPacket packet = f.Deserialize(m) as ProtocolPacket;
                packets.Add(packet);
            };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.SendData(CreateInfoPacket()));

            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendSensorsData(sensorsData.temperatureValue,
                                                   sensorsData.skinResistanceValue,
                                                   sensorsData.motionValue,
                                                   sensorsData.pulseValue));
            Thread.Sleep(200);

            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(ProtocolPacketType.ProtocolInfoPacket, packets[0].packetType);
            Assert.AreEqual(ProtocolPacketType.ServerInfoPacket, packets[1].packetType);
            Assert.AreEqual(ProtocolPacketType.SensorsDataPacket, packets[2].packetType);

            SensorsDataEventArgs receivedData = packets[2].serializedData as SensorsDataEventArgs;
            double eps = 0.0001;
            Assert.AreEqual(sensorsData.motionValue,            receivedData.motionValue,           eps);
            Assert.AreEqual(sensorsData.pulseValue,             receivedData.pulseValue,            eps);
            Assert.AreEqual(sensorsData.skinResistanceValue,    receivedData.skinResistanceValue,   eps);
            Assert.AreEqual(sensorsData.temperatureValue,       receivedData.temperatureValue,      eps);
        }

        [TestMethod]
        public void ProtocolSendsVoiceWindow()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            List<ProtocolPacket> packets = new List<ProtocolPacket>();

            Assert.IsTrue(protocol.Bind());

            protocol.ClientConnected += (s, e) => ev.Set();
            client.PacketReceived += (s, e) =>
            {
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream m = new MemoryStream(e.data as byte[]);
                ProtocolPacket packet = f.Deserialize(m) as ProtocolPacket;
                packets.Add(packet);
            };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.SendData(CreateInfoPacket()));

            byte[] voiceData = new byte[44100];
            for (int i = 0; i < voiceData.Length; ++i)
                voiceData[i] = (byte)i;

            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendVoiceWindow(voiceData));
            Thread.Sleep(200);

            Assert.AreEqual(3, packets.Count);
            Assert.AreEqual(ProtocolPacketType.ProtocolInfoPacket,  packets[0].packetType);
            Assert.AreEqual(ProtocolPacketType.ServerInfoPacket,    packets[1].packetType);
            Assert.AreEqual(ProtocolPacketType.VoiceWindowPacket,   packets[2].packetType);

            VoiceWindowDataEventArgs receivedData = packets[2].serializedData as VoiceWindowDataEventArgs;
            for (int i = 0; i < receivedData.data.Length; ++i)
                Assert.AreEqual(voiceData[i], receivedData.data[i]);
        }

        [TestMethod]
        public void ProtocolSendSensorsDataFailed()
        {
            Assert.IsTrue(protocol.Bind());
            Assert.IsFalse(protocol.SendSensorsData(motionValue: 1.0, pulseValue: 64.0, skinResistanceValue: 100500.0, temperatureValue: 36.6));
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
        public void ProtocolSendsNameAfterConnectionComplete()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            List<ProtocolPacket> packets = new List<ProtocolPacket>();

            Assert.IsTrue(protocol.Bind());

            protocol.ClientConnected += (s, e) => ev.Set();
            client.PacketReceived += (s, e) =>
            {
                BinaryFormatter f = new BinaryFormatter();
                MemoryStream m = new MemoryStream(e.data as byte[]);
                ProtocolPacket packet = f.Deserialize(m) as ProtocolPacket;
                packets.Add(packet);
            };

            Assert.IsTrue(client.Connect("localhost", protocolPort));

            // client info packet
            Assert.IsTrue(client.SendData(CreateInfoPacket()));
            Assert.IsTrue(ev.WaitOne(waitingTimeout));

            Thread.Sleep(200);

            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(ProtocolPacketType.ProtocolInfoPacket, packets[0].packetType);
            Assert.AreEqual(ProtocolPacketType.ServerInfoPacket, packets[1].packetType);

            ServerInfoEventArgs serverInfo = packets[1].serializedData as ServerInfoEventArgs;
            Assert.AreEqual(serverName, serverInfo.serverName);
        }

        [TestMethod]
        public void ProtocolReceivesChatMessage()
        {
            ManualResetEvent connected = new ManualResetEvent(false);
            ManualResetEvent messageReceived = new ManualResetEvent(false);

            ClientChatMessageEventArgs args = null;

            Assert.IsTrue(protocol.Bind());
            protocol.ClientConnected += (s, e) => connected.Set();
            protocol.ChatMessageReceived += (s, e) => { messageReceived.Set(); args = e; };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.SendData(CreateInfoPacket()));
            Assert.IsTrue(connected.WaitOne(waitingTimeout));

            // create chat message packet
            string message = "Hello!";
            ClientChatMessageEventArgs sentArgs = new ClientChatMessageEventArgs { message = message };
            ProtocolPacket packet = new ProtocolPacket()
            {
                packetType = ProtocolPacketType.ChatMessagePacket,
                serializedData = sentArgs
            };

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, packet);

            Assert.IsTrue(client.SendData(m.GetBuffer()));
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
            protocol.SettingsReceived += (s, e) => { messageReceived.Set(); args = e; };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.SendData(CreateInfoPacket()));
            Assert.IsTrue(connected.WaitOne(waitingTimeout));

            // create packet
            int channelsCount = 2;
            ChannelDescription[] channelDesc = new ChannelDescription[2];
            for (int i = 0; i < channelsCount; ++i)
                channelDesc[i] = new ChannelDescription(10.0, 20.0, 30.0, true);

            NoiseDescription noiseDesc = new NoiseDescription(10.0, 20.0);
            SettingsDataEventArgs sentArgs = new SettingsDataEventArgs
            {
                channels = channelDesc,
                noise = noiseDesc
            };

            ProtocolPacket packet = new ProtocolPacket()
            {
                packetType = ProtocolPacketType.SoundSettingsPacket,
                serializedData = sentArgs
            };

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, packet);

            Assert.IsTrue(client.SendData(m.GetBuffer()));
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));

            Assert.AreEqual(channelDesc.Length, args.channels.Length);
            for (int i = 0; i < channelsCount; ++i)
            {
                Assert.AreEqual(channelDesc[i].carrierFrequency, args.channels[i].carrierFrequency, 0.0001);
                Assert.AreEqual(channelDesc[i].differenceFrequency, args.channels[i].differenceFrequency, 0.0001);
                Assert.AreEqual(channelDesc[i].volume, args.channels[i].volume, 0.0001);
                Assert.AreEqual(channelDesc[i].enabled, args.channels[i].enabled);
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
            protocol.VoiceWindowReceived += (s, e) => { voiceReceived.Set(); args = e; };

            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.SendData(CreateInfoPacket()));
            Assert.IsTrue(connected.WaitOne(waitingTimeout));

            byte[] voiceData = new byte[44100];
            for (int i = 0; i < voiceData.Length; ++i)
                voiceData[i] = (byte)i;

            // create chat message packet
            VoiceWindowDataEventArgs sentArgs = new VoiceWindowDataEventArgs { data = voiceData };
            ProtocolPacket packet = new ProtocolPacket()
            {
                packetType = ProtocolPacketType.VoiceWindowPacket,
                serializedData = sentArgs
            };

            MemoryStream m = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(m, packet);

            Assert.IsTrue(client.SendData(m.GetBuffer()));
            Assert.IsTrue(voiceReceived.WaitOne(waitingTimeout));
            Assert.IsTrue(Enumerable.SequenceEqual(voiceData, args.data));
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
