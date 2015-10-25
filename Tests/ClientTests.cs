using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;

namespace Tests
{
    [TestClass]
    public class ClientTests
    {
        IServerConnectionInterface server = null;
        IClientConnectionInterface client = null;
        int port = 11000;

        public void StartServer()
        {
            server = new InternetServerConnectionInterface();
            bool serverStartResult = server.StartListening("localhost", port);
            Assert.IsTrue(serverStartResult);
        }

        public void EndServer()
        {
            server.Shutdown();
            server = null;
        }

        public void StartClient()
        {
            client = new InternetClientConnectionInterface();
            bool clientStartResult = client.Connect("localhost", port);
            Assert.IsTrue(clientStartResult);
        }

        public void EndClient()
        {
            client.Disconnect();
            client = null;
        }

        [TestInitialize]
        public void Initialize()
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
        public void ClientStartTest()
        {
            StartServer();
            StartClient();
            EndClient();
            EndServer();
        }

        [TestMethod]
        public void ConnectionTest()
        {
            // StartServer();
            // StartClient();
        }
    }
}
