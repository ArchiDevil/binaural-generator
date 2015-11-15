using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;

namespace Tests
{
    [TestClass]
    public class ServerProtocolTests
    {
        ServerProtocol protocol = null;
        InternetClientConnectionInterface client = null;

        int protocolPort = ServerProtocol.protocolPort;
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
            Assert.IsTrue(protocol.Bind("localhost"));
            Assert.IsTrue(client.Connect("localhost", protocolPort));
        }

        [TestMethod]
        public void CanProtocolStop()
        {
            Assert.IsTrue(protocol.Bind("localhost"));
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
                Assert.IsTrue(protocol.Bind("localhost"));
                Assert.IsTrue(client.Connect("localhost", protocolPort));
                client.Disconnect();
                protocol.Stop();
            }
        }

        [TestMethod]
        public void ProtocolEventOnClientConnected()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(protocol.Bind("localhost"));
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

            Assert.IsTrue(protocol.Bind("localhost"));
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
        public void ProtocolSendChatMessage()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(protocol.Bind("localhost"));
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendChatMessage("Hello"));
        }

        [TestMethod]
        public void ProtocolSendChatMessageFailed()
        {
            Assert.IsTrue(protocol.Bind("localhost"));
            Assert.IsFalse(protocol.SendChatMessage(null));
            Assert.IsFalse(protocol.SendChatMessage("Hello"));
            Assert.IsFalse(protocol.SendChatMessage(""));
        }

        [TestMethod]
        public void ProtocolSendSensorsData()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(protocol.Bind("localhost"));
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);

            SensorsDataEventArgs sensorsData = new SensorsDataEventArgs
            {
                motionValue = 1.0,
                pulseValue = 64.0,
                skinResistanceValue = 100500.0,
                temperatureValue = 36.6
            };

            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendSensorsData(sensorsData));
        }

        [TestMethod]
        public void ProtocolSendSensorsDataFailed()
        {
            Assert.IsTrue(protocol.Bind("localhost"));
            SensorsDataEventArgs sensorsData = new SensorsDataEventArgs
            {
                motionValue = 1.0,
                pulseValue = 64.0,
                skinResistanceValue = 100500.0,
                temperatureValue = 36.6
            };

            Assert.IsFalse(protocol.SendSensorsData(null));
            Assert.IsFalse(protocol.SendSensorsData(sensorsData));
        }

        [TestMethod]
        public void ProtocolSendVoiceWindow()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(protocol.Bind("localhost"));
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);

            VoiceWindowDataEventArgs voiceData = new VoiceWindowDataEventArgs();
            for (int i = 0; i < voiceData.data.Length; ++i)
                voiceData.data[i] = (byte)i;

            Assert.IsTrue(ev.WaitOne(waitingTimeout));
            Assert.IsTrue(protocol.SendVoiceWindow(voiceData));
        }

        [TestMethod]
        public void ProtocolSendVoiceWindowFailed()
        {
            Assert.IsTrue(protocol.Bind("localhost"));
            VoiceWindowDataEventArgs voiceData = new VoiceWindowDataEventArgs();
            for (int i = 0; i < voiceData.data.Length; ++i)
                voiceData.data[i] = (byte)i;

            Assert.IsFalse(protocol.SendVoiceWindow(null));
            Assert.IsFalse(protocol.SendVoiceWindow(voiceData));

            voiceData.data = null;
            Assert.IsFalse(protocol.SendVoiceWindow(voiceData));
        }

        [TestMethod]
        public void ProtocolReceiveChatMessage()
        {
            ManualResetEvent connected = new ManualResetEvent(false);
            ManualResetEvent messageReceived = new ManualResetEvent(false);

            ClientChatMessageEventArgs args = null;

            Assert.IsTrue(protocol.Bind("localhost"));
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

            byte[] msg = new byte[sizeof(byte) + sizeof(int) + m.GetBuffer().Length];
            msg[0] = 4; // chat message packet
            BitConverter.GetBytes(m.GetBuffer().Length).CopyTo(msg, 1);
            m.GetBuffer().CopyTo(msg, 5);
            Assert.IsTrue(client.Send(msg) > 0);
            Assert.IsTrue(messageReceived.WaitOne(waitingTimeout));
            Assert.AreEqual(message, args.message);
        }

        [TestMethod]
        public void NOT_IMPLEMENTED_ProtocolReceiveSettingsEvent()
        {
        }

        [TestMethod]
        public void ProtocolReceiveVoiceWindow()
        {
            ManualResetEvent connected = new ManualResetEvent(false);
            ManualResetEvent voiceReceived = new ManualResetEvent(false);
            VoiceWindowDataEventArgs args = null;

            Assert.IsTrue(protocol.Bind("localhost"));
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

            byte[] msg = new byte[1 + 4 + m.GetBuffer().Length];
            msg[0] = 5; // voice message packet
            BitConverter.GetBytes(m.GetBuffer().Length).CopyTo(msg, 1);
            m.GetBuffer().CopyTo(msg, 5);

            Assert.IsTrue(client.Send(msg) > 0);
            Assert.IsTrue(voiceReceived.WaitOne(waitingTimeout));
            Assert.IsTrue(Enumerable.SequenceEqual(voiceData, args.data));
        }

        [TestMethod]
        public void ProtocolSendsNameAfterConnectionComplete()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(protocol.Bind("localhost"));
            protocol.ClientConnected += (s, e) => ev.Set();
            Assert.IsTrue(client.Connect("localhost", protocolPort));

            // client info packet
            Assert.IsTrue(client.Send(CreateInfoPacket()) > 0);
            Assert.IsTrue(ev.WaitOne(waitingTimeout));

            // here protocol information and client name should be
            byte[] msg = new byte[1024];
            int totalCount = 0;
            for (int i = 0; i < 5; ++i)
            {
                byte[] tmp = new byte[1024];
                int count = client.Receive(tmp, 1000);
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

            Debug.Print("Total count: " + totalCount.ToString());

            BinaryFormatter f = new BinaryFormatter();
            // try to read packets
            byte packetType = msg[0];
            Assert.IsTrue(packetType == 1); // ProtocolInfo packet
            int packetSize = BitConverter.ToInt32(msg, 1);
            Assert.IsTrue(packetSize > 0);

            MemoryStream m = new MemoryStream(msg, 5, packetSize);

            // reading next packet...
            msg = msg.Skip(1 + sizeof(int) + packetSize).Take(totalCount - 1 - sizeof(int) - packetSize).ToArray();
            packetType = msg[0];
            Assert.IsTrue(packetType == 3, "Wrong packet type: " + packetType.ToString()); // ServerInfo packet
            packetSize = BitConverter.ToInt32(msg, 1);
            Assert.IsTrue(packetSize > 0);

            m = new MemoryStream(msg, 5, packetSize);
            ServerInfoEventArgs serverInfo = (ServerInfoEventArgs)f.Deserialize(m);
            Assert.AreEqual(serverName, serverInfo.serverName);
        }
    }
}
