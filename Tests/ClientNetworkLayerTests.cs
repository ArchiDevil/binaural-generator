using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;

namespace Tests
{
    [TestClass]
    public sealed class ClientNetworkLayerTests : IDisposable
    {
        InternetServerConnectionInterface server = null;
        InternetClientConnectionInterface client = null;
        ushort port = 11000;

        public void StartServer(string bindingPoint = "localhost")
        {
            server = new InternetServerConnectionInterface();

            bool serverStartResult = false;
            if (bindingPoint.Length != 0)
                serverStartResult = server.StartListening(bindingPoint, port);
            else
                serverStartResult = server.StartListening(port);

            Assert.IsTrue(serverStartResult);
        }

        public void EndServer()
        {
            server.Shutdown();
            server = null;
        }

        public void StartClient(string address = "localhost")
        {
            client = new InternetClientConnectionInterface();
            bool clientStartResult = client.Connect(address, port);
            Assert.IsTrue(clientStartResult);
        }

        public void EndClient()
        {
            client.Disconnect();
            client = null;
        }

        [TestMethod]
        public void ClientStartTest()
        {
            StartServer();
            StartClient();
            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ClientCheckConnectedTest()
        {
            StartServer();
            StartClient();
            Assert.IsTrue(client.IsConnected());
            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ClientMultiStartTest()
        {
            StartServer("");

            StartClient("127.0.0.1");
            EndClient();

            StartClient("127.0.0.1");
            EndClient();

            EndServer();
        }

        [TestMethod]
        public void ClientSendingTest()
        {
            StartServer();
            StartClient();

            string message = "Hello!";
            int count = client.Send(Encoding.ASCII.GetBytes(message));
            Assert.AreEqual(message.Length, count);

            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ClientHeavySendingTest()
        {
            StartServer();
            StartClient();

            int appendsCount = 1024 * 1024;
            StringBuilder b = new StringBuilder("Hello, World!");
            for (int i = 0; i < appendsCount; ++i)
            {
                b.Append(", append value! =)");
            }

            string message = b.ToString();
            int count = client.Send(Encoding.ASCII.GetBytes(message));
            Assert.AreEqual(message.Length, count);

            EndServer();
            EndClient();
        }

        [TestMethod]
        public void ClientSendingFailedTest()
        {
            client = new InternetClientConnectionInterface();
            int count = client.Send(Encoding.ASCII.GetBytes("Hello"));
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void ClientReceivingTest()
        {
            StartServer();
            StartClient();

            string message = "Hello";
            server.Send(Encoding.ASCII.GetBytes(message));

            byte[] received = new byte[1024];
            int count = client.Receive(received);

            Assert.AreEqual(message.Length, count);
            if (Encoding.ASCII.GetString(received).IndexOf(message) == -1)
                Assert.Fail();

            EndServer();
            EndClient();
        }

        [TestMethod]
        public void ClientReceivingTimeoutTest()
        {
            StartServer();
            StartClient();

            string message = "Hello";
            server.Send(Encoding.ASCII.GetBytes(message));

            byte[] received = new byte[1024];
            int count = client.Receive(received, 1000);

            Assert.AreEqual(message.Length, count);
            if (Encoding.ASCII.GetString(received).IndexOf(message) == -1)
                Assert.Fail();

            EndServer();
            EndClient();
        }

        [TestMethod]
        public void ClientHeavyReceivingTest()
        {
            StartServer();
            StartClient();

            int appendsCount = 16384;
            StringBuilder b = new StringBuilder("Hello, World!");
            for (int i = 0; i < appendsCount; ++i)
            {
                b.Append(", append value! =)");
            }

            string message = b.ToString();
            int count = server.Send(Encoding.ASCII.GetBytes(message));
            Assert.AreEqual(message.Length, count);

            int bufferSize = 1024 * 1024 * 20;
            byte[] buffer = new byte[1024 * 1024 * 20]; // 20 mb buffer
            int offset = 0;
            while (offset != message.Length)
            {
                count = client.Receive(buffer, offset, bufferSize - offset);
                offset += count;
            }
            count = offset;
            Assert.AreEqual(message.Length, count);

            EndServer();
            EndClient();
        }

        [TestMethod]
        public void ClientReceivingFailedTest()
        {
            client = new InternetClientConnectionInterface();
            byte[] msg = new byte[1024];
            int count = client.Receive(msg);
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void ClientCanReconnect()
        {
            StartServer();
            StartClient();

            int count = client.Send(Encoding.ASCII.GetBytes("Hello"));
            Assert.AreEqual(5, count);
            client.Disconnect();

            client.Connect("localhost", port);
            count = client.Send(Encoding.ASCII.GetBytes("Hello"));
            Assert.AreEqual(5, count);

            EndClient();
            EndServer();
        }

        public void Dispose()
        {
            if (client != null)
                client.Dispose();

            if (server != null)
                server.Dispose();
        }
    }
}
