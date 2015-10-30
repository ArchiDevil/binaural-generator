using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;

namespace Tests
{
    [TestClass]
    public class ServerClientTests
    {
        InternetServerConnectionInterface server = null;
        InternetClientConnectionInterface client = null;
        int port = 11000;

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

        [TestCleanup]
        public void Shutdown()
        {
            if (server != null)
            {
                EndServer();
                server = null;
            }

            if (client != null)
            {
                EndClient();
                client = null;
            }
        }

        [TestMethod]
        public void ServerStartMultiBindTest()
        {
            StartServer("");
            EndServer();
        }

        [TestMethod]
        public void ServerStartSingleBindTest()
        {
            StartServer();
            EndServer();
        }

        [TestMethod]
        public void ServerSendingTest()
        {
            StartServer();
            StartClient();

            string message = "Hello, World!";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = server.Send(msg);
            Assert.AreEqual(message.Length, count);

            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ServerHeavySendingTest()
        {
            StartServer();
            StartClient();

            int appendsCount = 1024 * 1024; // is about 10 mbytes of sent data
            StringBuilder b = new StringBuilder("Hello, World!");
            for (int i = 0; i < appendsCount; ++i)
            {
                b.Append(", append value! =)");
            }

            string message = b.ToString();
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = server.Send(msg);
            Assert.AreEqual(message.Length, count);

            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ServerFailedSendingTest()
        {
            StartServer();

            string message = "Hello, World!";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = server.Send(msg);
            Assert.AreEqual(0, count);

            EndServer();
        }

        [TestMethod]
        public void ServerFailedSendingAfterDisconnectTest()
        {
            StartServer();
            StartClient();

            string message = "Hello, World!";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = server.Send(msg);
            Assert.AreEqual(message.Length, count);

            EndClient();

            count = server.Send(msg);
            Assert.AreEqual(0, count);

            EndServer();
        }

        [TestMethod]
        public void ServerReceivingTest()
        {
            StartServer();
            StartClient();

            string message = "Hello, World!";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = 0;
            count = client.Send(msg);
            Assert.AreEqual(message.Length, count);

            count = server.Receive(msg);
            Assert.AreEqual(message, Encoding.ASCII.GetString(msg));

            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ServerHeavyReceivingTest()
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
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = 0;
            count = client.Send(msg);
            Assert.AreEqual(message.Length, count);

            count = server.Receive(msg);
            Assert.AreEqual(message, Encoding.ASCII.GetString(msg));

            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ServerFailedReceivingTest()
        {
            StartServer();

            byte[] msg = new byte[1024];
            int count = server.Receive(msg);
            Assert.AreEqual(0, count);

            EndServer();
        }

        [TestMethod]
        public void ServerFailedReceivingAfterDisconnectTest()
        {
            StartServer();
            StartClient();

            string message = "Hello, World!";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = 0;
            count = client.Send(msg);
            Assert.AreEqual(message.Length, count);

            count = server.Receive(msg);
            Assert.AreEqual(message, Encoding.ASCII.GetString(msg));

            EndClient();

            count = server.Receive(msg);
            Assert.AreEqual(0, count);

            EndServer();
        }

        [TestMethod]
        public void ServerIsListeningTest()
        {
            server = new InternetServerConnectionInterface();
            Assert.AreEqual(false, server.IsListening());

            server.StartListening(port);
            Assert.AreEqual(true, server.IsListening());

            server.Shutdown();
            Assert.AreEqual(false, server.IsListening());
            server = null;
        }

        [TestMethod]
        public void ServerCanAcceptReconnections()
        {
            StartServer();
            StartClient();
            for(int i = 0; i < 10; i++)
            {
                client.Disconnect();
                Assert.IsTrue(client.Connect("localhost", port));
            }
            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ServerCanRelisten()
        {
            StartServer();
            StartClient();

            client.Disconnect();
            server.Shutdown();

            Assert.IsTrue(server.StartListening("localhost", port));
            Assert.IsTrue(client.Connect("localhost", port));

            EndClient();
            EndServer();
        }

        // client tests

        [TestMethod]
        public void ClientStartTest()
        {
            StartServer();
            StartClient();
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
    }
}
