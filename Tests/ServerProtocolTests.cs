using System;
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
            ManualResetEvent e = new ManualResetEvent(false);

            protocol.Bind("localhost");
            protocol.ClientConnected += (args) => e.Set();
            client.Connect("localhost", protocolPort);

            if (!e.WaitOne(5000))
                Assert.Fail();
        }

        [TestMethod]
        public void ProtocolEventOnClientConnectedCheckInfo()
        {
            ManualResetEvent e = new ManualResetEvent(false);
            ClientInfo receivedInfo = null;

            protocol.Bind("localhost");
            protocol.ClientConnected += (info) => { e.Set(); receivedInfo = info; };
            client.Connect("localhost", protocolPort);

            if (!e.WaitOne(5000))
                Assert.Fail();

            Assert.AreNotEqual(null, receivedInfo);
            Assert.AreEqual("Test client", receivedInfo.clientName);
        }

        [TestMethod]
        public void ProtocolSendChatMessage()
        {
            ManualResetEvent e = new ManualResetEvent(false);

            protocol.Bind("localhost");
            protocol.ClientConnected += (args) => e.Set();
            client.Connect("localhost", protocolPort);

            if (!e.WaitOne(5000))
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
            ManualResetEvent e = new ManualResetEvent(false);

            protocol.Bind("localhost");
            protocol.ClientConnected += (args) => e.Set();
            client.Connect("localhost", protocolPort);

            SensorsData sensorsData = new SensorsData
            {
                motionValue = 1.0,
                pulseValue = 64.0,
                skinResistanceValue = 100500.0,
                temperatureValue = 36.6
            };

            if (!e.WaitOne(5000))
                Assert.Fail();

            Assert.IsTrue(protocol.SendSensorsData(sensorsData));
        }

        [TestMethod]
        public void ProtocolSendSensorsDataFailed()
        {
            protocol.Bind("localhost");
            SensorsData sensorsData = new SensorsData
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
            ManualResetEvent e = new ManualResetEvent(false);

            protocol.Bind("localhost");
            protocol.ClientConnected += (args) => e.Set();
            client.Connect("localhost", protocolPort);

            VoiceWindowData voiceData = new VoiceWindowData();
            for(int i = 0; i < voiceData.data.Length; ++i)
            {
                voiceData.data[i] = (byte)i;
            }

            if (!e.WaitOne(5000))
                Assert.Fail();

            Assert.IsTrue(protocol.SendVoiceWindow(voiceData));
        }

        [TestMethod]
        public void ProtocolSendVoiceWindowFailed()
        {
            protocol.Bind("localhost");
            VoiceWindowData voiceData = new VoiceWindowData();
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
