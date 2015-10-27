using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void ServerFailedSendingTest()
        {
            server = new InternetServerConnectionInterface();
            string message = "Hello, World!";
            byte[] msg = Encoding.ASCII.GetBytes(message);
            int count = server.Send(msg);
            Assert.AreEqual(0, count);
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
        public void ServerFailedReceivingTest()
        {
            server = new InternetServerConnectionInterface();
            byte[] msg = new byte[1024];
            int count = server.Receive(msg);
            Assert.AreEqual(0, count);
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

            Console.WriteLine("Starting client on 127.0.0.1");
            StartClient("127.0.0.1");

            Console.WriteLine("Closing client");
            EndClient();

            Console.WriteLine("Starting client on 127.0.0.1");
            StartClient("127.0.0.1");

            Console.WriteLine("Closing client");
            EndClient();

            EndServer();
        }
    }
}
