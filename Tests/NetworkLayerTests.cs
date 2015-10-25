using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;

namespace Tests
{
    [TestClass]
    public class ClientConnectionInterfaceTests
    {
        IServerConnectionInterface server = null;
        IClientConnectionInterface client = null;
        int port = 11000;

        public void StartServer()
        {
            server = new InternetServerConnectionInterface();
            bool serverStartResult = server.Start(port);
            Assert.IsTrue(serverStartResult);
        }

        public void StartClient()
        {
            client = new InternetClientConnectionInterface();
            bool clientStartResult = client.Connect("localhost", port);
            Assert.IsTrue(clientStartResult);
        }

        [TestMethod]
        public void ServerStartTest()
        {
            StartServer();
        }

        [TestMethod]
        public void ClientStartTest()
        {
            StartClient();
        }

        [TestMethod]
        public void ConnectionTest()
        {
            StartServer();
            StartClient();
        }
    }
}
