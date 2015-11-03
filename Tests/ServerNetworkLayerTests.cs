using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;

namespace Tests
{
    [TestClass]
    public class ServerNetworkLayerTests
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
        public void ServerClientConnectedEventTest()
        {
            bool flag = false;

            StartServer();
            server.ClientConnected += () => { flag = true; };

            StartClient();

            // wait till event receive
            for(int i = 0; i < 1000000; ++i)
                if (flag)
                    break;

            Assert.IsTrue(flag);

            EndClient();
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
        public void ServerReceivingTimeoutTest()
        {
            StartServer();
            StartClient();

            string message = "Hello, World!";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = 0;
            count = client.Send(msg);
            Assert.AreEqual(message.Length, count);

            count = server.Receive(msg, 1000);
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
        public void ServerIsClientConnectedTest()
        {
            StartServer();
            Assert.IsFalse(server.IsClientConnected());

            StartClient();
            System.Threading.Thread.Sleep(200);
            Assert.IsTrue(server.IsClientConnected());
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
    }
}
