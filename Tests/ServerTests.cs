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
    public class ServerTests
    {
        IServerConnectionInterface server = null;
        int port = 11000;

        public void StartServer(string bindingPoint = "")
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

        [TestMethod]
        public void ServerStartTest()
        {
            StartServer();
            EndServer();
        }

        [TestMethod]
        public void ServerStartBindingPointTest()
        {
            StartServer("localhost");
            EndServer();
        }
    }
}
