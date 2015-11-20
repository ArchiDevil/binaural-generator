using System;
using System.Text;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;
using NetworkLayer.Protocol;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class ClientProtocolTests
    {
        ClientProtocol protocol = null;
        InternetServerConnectionInterface server = null;

        ushort protocolPort = 31012;
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
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
        }

        [TestMethod]
        public void CanProtocolConnectTwice()
        {
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            Assert.IsTrue(protocol.Connect("localhost"));
        }

        [TestMethod]
        public void CanProtocolDisconnect()
        {
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            protocol.Disconnect();
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
            catch(Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CanProtocolReconnect()
        {
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));
            protocol.Disconnect();
            Assert.IsTrue(protocol.Connect("localhost"));
        }

        [TestMethod]
        public void ClientSendsInfoAfterConnect()
        {
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
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
        public void NOT_IMPLEMENTED_ClientSendsSignalSettings()
        {
            //Assert.IsTrue(server.StartListening("localhost", protocolPort));
            //Assert.IsTrue(protocol.Connect("localhost"));
            //Assert.IsTrue(protocol.SendSignalSettings(36.6, 100500.0, 10.0, 60.0));

            //byte[] buffer = new byte[4096];
            //int count = server.Receive(buffer, 5000);
            //Assert.IsTrue(count > 0);
            //buffer = buffer.Take(count).ToArray();
            //BinaryFormatter b = new BinaryFormatter();
            //MemoryStream m = new MemoryStream(buffer);
            //Signal args = (SettingsDataEventArgs)b.Deserialize(m);
            //args
        }

        [TestMethod]
        public void NOT_IMPLEMENTED_ClientSendSignalSettingsFailed()
        {
        }

        [TestMethod]
        public void ClientSendsVoiceWindow()
        {
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
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
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            byte[] b = new byte[0];
            Assert.IsFalse(protocol.SendVoiceWindow(b));
            Assert.IsFalse(protocol.SendVoiceWindow(null));
        }

        [TestMethod]
        public void ClientSendsChatMessage()
        {
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
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
            Assert.IsTrue(server.StartListening("localhost", protocolPort));
            Assert.IsTrue(protocol.Connect("localhost"));

            Assert.IsFalse(protocol.SendChatMessage(null));
            Assert.IsFalse(protocol.SendChatMessage(""));
        }
    }
}
