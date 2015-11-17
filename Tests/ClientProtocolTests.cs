using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NetworkLayer;
using NetworkLayer.Protocol;

namespace Tests
{
    [TestClass]
    public class ClientProtocolTests
    {
        ClientProtocol protocol = null;
        InternetServerConnectionInterface server = null;

        int protocolPort = 31012;
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
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void CanProtocolDisconnect()
        {
            Assert.Fail("Not implemented yet");
        }

        [TestMethod]
        public void CanProtocolReconnect()
        {
            Assert.Fail("Not implemented yet");
        }
    }
}
