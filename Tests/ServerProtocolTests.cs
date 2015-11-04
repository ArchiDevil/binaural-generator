using System;
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
            protocol = new ServerProtocol();
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

            protocol.Bind("localhost");
            protocol.ClientConnected += (s, e) => ev.Set();
            client.Connect("localhost", protocolPort);

            // client info packet
            client.Send(CreateInfoPacket());

            if (!ev.WaitOne(50000))
                Assert.Fail();
        }

        [TestMethod]
        public void ProtocolEventOnClientConnectedCheckInfo()
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            ClientInfoEventArgs receivedInfo = null;

            protocol.Bind("localhost");
            protocol.ClientConnected += (s, e) => { ev.Set(); receivedInfo = e; };
            client.Connect("localhost", protocolPort);

            // client info packet
            string message = "Test client";
            client.Send(CreateInfoPacket());

            if (!ev.WaitOne(5000))
                Assert.Fail();

            Assert.AreNotEqual(null, receivedInfo);
            Assert.AreEqual(message, receivedInfo.clientName);
        }

        [TestMethod]
        public void ProtocolSendChatMessage()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            protocol.Bind("localhost");
            protocol.ClientConnected += (s, e) => ev.Set();
            client.Connect("localhost", protocolPort);
            client.Send(CreateInfoPacket());

            if (!ev.WaitOne(5000))
                Assert.Fail();

            Assert.IsTrue(protocol.SendChatMessage("Hello"));
        }

        [TestMethod]
        public void ProtocolSendChatMessageFailed()
        {
            protocol.Bind("localhost");
            Assert.IsFalse(protocol.SendChatMessage(null));
            Assert.IsFalse(protocol.SendChatMessage("Hello"));
            Assert.IsFalse(protocol.SendChatMessage(""));
        }

        [TestMethod]
        public void ProtocolSendSensorsData()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            protocol.Bind("localhost");
            protocol.ClientConnected += (s, e) => ev.Set();
            client.Connect("localhost", protocolPort);
            client.Send(CreateInfoPacket());

            SensorsDataEventArgs sensorsData = new SensorsDataEventArgs
            {
                motionValue = 1.0,
                pulseValue = 64.0,
                skinResistanceValue = 100500.0,
                temperatureValue = 36.6
            };

            if (!ev.WaitOne(5000))
                Assert.Fail();

            Assert.IsTrue(protocol.SendSensorsData(sensorsData));
        }

        [TestMethod]
        public void ProtocolSendSensorsDataFailed()
        {
            protocol.Bind("localhost");
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

            protocol.Bind("localhost");
            protocol.ClientConnected += (s, e) => ev.Set();
            client.Connect("localhost", protocolPort);
            client.Send(CreateInfoPacket());

            VoiceWindowDataEventArgs voiceData = new VoiceWindowDataEventArgs();
            for(int i = 0; i < voiceData.data.Length; ++i)
            {
                voiceData.data[i] = (byte)i;
            }

            if (!ev.WaitOne(5000))
                Assert.Fail();

            Assert.IsTrue(protocol.SendVoiceWindow(voiceData));
        }

        [TestMethod]
        public void ProtocolSendVoiceWindowFailed()
        {
            protocol.Bind("localhost");
            VoiceWindowDataEventArgs voiceData = new VoiceWindowDataEventArgs();
            for (int i = 0; i < voiceData.data.Length; ++i)
            {
                voiceData.data[i] = (byte)i;
            }

            Assert.IsFalse(protocol.SendVoiceWindow(null));
            Assert.IsFalse(protocol.SendVoiceWindow(voiceData));

            voiceData.data = null;
            Assert.IsFalse(protocol.SendVoiceWindow(voiceData));
        }
    }
}
