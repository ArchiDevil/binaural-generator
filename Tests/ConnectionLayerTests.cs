using System;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkLayer.Client;
using NetworkLayer.Server;

namespace Tests
{
    [TestClass]
    public sealed class ConnectionLayerTests
    {
        IClientConnectionLayer _client = null;
        IServerConnectionLayer _server = null;

        const ushort _testPort = 10000;

        [TestInitialize]
        public void Initialize()
        {
            _client = new ClientNetworkConnectionLayer();
            _server = new ServerNetworkConnectionLayer();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client = null;
            _server = null;
        }

        [TestMethod]
        public void CanBindAndStop()
        {
            Assert.IsTrue(_server.Bind(_testPort));
            _server.Stop();
        }

        [TestMethod]
        public void CanListen()
        {
            Assert.IsTrue(_server.Bind(_testPort));
            Assert.IsTrue(_server.IsListening());
            Assert.IsTrue(_server.Stop());
        }

        [TestMethod]
        public void CanConnectionBeEstablished()
        {
            Assert.IsTrue(_server.Bind(_testPort));
            Assert.IsTrue(_client.Connect("localhost", _testPort));
            Assert.IsTrue(_client.Disconnect());
            Assert.IsTrue(_server.Stop());
        }

        [TestMethod]
        public void ServerSendsEventOnConnection()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(_server.Bind(_testPort));
            _server.ClientConnected += (x, y) => ev.Set();
            Assert.IsTrue(_client.Connect("localhost", _testPort));

            Assert.IsTrue(ev.WaitOne(100));

            Assert.IsTrue(_client.Disconnect());
            Assert.IsTrue(_server.Stop());
        }

        [TestMethod]
        public void ServerSendsEventOnDisconnection()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(_server.Bind(_testPort));
            _server.ClientDisconnected += x => ev.Set();
            Assert.IsTrue(_client.Connect("localhost", _testPort));
            Assert.IsTrue(_client.Disconnect());
            Assert.IsTrue(ev.WaitOne(100));
            Assert.IsTrue(_server.Stop());
        }

        //[TestMethod]
        //public void ServerSendsEventOnFailedConnection()
        //{
        //    ManualResetEvent ev = new ManualResetEvent(false);

        //    Assert.IsTrue(_server.Bind(_testPort));
        //    _server.ConnectionLost += x => ev.Set();
        //    Assert.IsTrue(_client.Connect("localhost", _testPort));
        //    _client = null;
        //    GC.Collect();
        //    Assert.IsTrue(ev.WaitOne(1000));
        //    Assert.IsTrue(_server.Stop());
        //}

        [TestMethod]
        public void ClientSendsEventOnDisconnection()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(_server.Bind(_testPort));
            _client.ConnectionFinished += x => ev.Set();
            Assert.IsTrue(_client.Connect("localhost", _testPort));
            Assert.IsTrue(_server.Stop());
            Assert.IsTrue(ev.WaitOne(100));
            Assert.IsTrue(_client.Disconnect());
        }

        //[TestMethod]
        //public void ClientSendsEventOnFailedConnection()
        //{
        //    ManualResetEvent ev = new ManualResetEvent(false);

        //    Assert.IsTrue(_server.Bind(_testPort));
        //    _client.ConnectionLost += x => ev.Set();
        //    Assert.IsTrue(_client.Connect("localhost", _testPort));
        //    _server = null;
        //    GC.Collect();
        //    _client.SendData(new byte[1]);
        //    Assert.IsTrue(ev.WaitOne(1000));
        //    Assert.IsTrue(_client.Disconnect());
        //}

        [TestMethod]
        public void ClientFailedOnConnectionToEmpty()
        {
            Assert.IsFalse(_client.Connect("localhost", _testPort));
        }

        [TestMethod]
        public void ClientToServerDataSending()
        {
            byte[] bytes = new byte[4] { 1, 2, 3, 4 };
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(_server.Bind(_testPort));
            _server.PacketReceived += (x, y) =>
            {
                Assert.IsTrue(Enumerable.SequenceEqual((y.data as byte[]), bytes));
                ev.Set();
            };

            _client.Connect("localhost", _testPort);
            _client.SendData(bytes);

            Assert.IsTrue(ev.WaitOne(100));

            _client.Disconnect();
            Assert.IsTrue(_server.Stop());
        }

        [TestMethod]
        public void ClientToServerHugeDataSending()
        {
            // 64 MBytes of pure numbers
            byte[] bytes = new byte[1024 * 1024 * 64];
            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)i;
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(_server.Bind(_testPort));
            _server.PacketReceived += (x, y) =>
            {
                Assert.IsTrue(Enumerable.SequenceEqual((y.data as byte[]), bytes));
                ev.Set();
            };

            _client.Connect("localhost", _testPort);
            _client.SendData(bytes);

            Assert.IsTrue(ev.WaitOne(10000));

            _client.Disconnect();
            Assert.IsTrue(_server.Stop());
        }

        [TestMethod]
        public void ServerToClientDataSending()
        {
            byte[] bytes = new byte[4] { 1, 2, 3, 4 };
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(_server.Bind(_testPort));
            _client.PacketReceived += (x, y) =>
            {
                Assert.IsTrue(Enumerable.SequenceEqual((y.data as byte[]), bytes));
                ev.Set();
            };

            _client.Connect("localhost", _testPort);
            Assert.IsTrue(_server.SendData(bytes));

            Assert.IsTrue(ev.WaitOne(100));

            _client.Disconnect();
            Assert.IsTrue(_server.Stop());
        }

        [TestMethod]
        public void ServerToClientHugeDataSending()
        {
            // 64 MBytes of pure numbers
            byte[] bytes = new byte[1024 * 1024 * 64];
            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)i;
            ManualResetEvent ev = new ManualResetEvent(false);

            Assert.IsTrue(_server.Bind(_testPort));
            _client.PacketReceived += (x, y) =>
            {
                Assert.IsTrue(Enumerable.SequenceEqual((y.data as byte[]), bytes));
                ev.Set();
            };

            _client.Connect("localhost", _testPort);
            Assert.IsTrue(_server.SendData(bytes));

            Assert.IsTrue(ev.WaitOne(15000));

            _client.Disconnect();
            Assert.IsTrue(_server.Stop());
        }
    }
}
