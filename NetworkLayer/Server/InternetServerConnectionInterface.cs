using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLayer
{
    public class InternetServerConnectionInterface : IServerConnectionInterface
    {
        string bindingPoint = string.Empty;
        int port = -1;
        List<Socket> listenerSockets = new List<Socket>();
        Socket client = null;
        ManualResetEvent stopExecution = new ManualResetEvent(false);
        Thread runnerThread = null;
        private bool isTermination = false;

        private void Work()
        {
            byte[] buffer = new byte[1024];

            int received = client.Receive(buffer);
            string message = Encoding.ASCII.GetString(buffer, 0, received);
            if (message.IndexOf("<EOF>") == -1)
                return;

            buffer = Encoding.ASCII.GetBytes("<EOF>");
            client.Send(buffer);

            while (true)
            {

            }
        }

        private void AsyncAcceptCallback(IAsyncResult result)
        {
            if (isTermination)
                return;

            Socket listener = result.AsyncState as Socket;
            client = listener.EndAccept(result);

            runnerThread = new Thread(Work);
            runnerThread.Start();
        }

        public bool StartListening(int port)
        {
            return StartListening("", port);
        }

        public bool StartListening(string bindingPoint, int port)
        {
            Shutdown();
            this.bindingPoint = bindingPoint;
            this.port = port;

            IPHostEntry ipHostInfo = null;
            if (bindingPoint.Length != 0)
                ipHostInfo = Dns.GetHostEntry(bindingPoint);
            else
                ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var address in ipHostInfo.AddressList)
            {
                if (address.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                IPEndPoint localEndPoint = new IPEndPoint(address, port);
                Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenerSockets.Add(listenerSocket);
                try
                {
                    listenerSocket.Bind(localEndPoint);
                    listenerSocket.Listen(1);
                    listenerSocket.BeginAccept(new AsyncCallback(AsyncAcceptCallback), listenerSocket);
                }
                catch (Exception e)
                {
                    Debug.Assert(false, e.Message);
                    return false;
                }
            }

            return true;
        }

        public void Shutdown()
        {
            isTermination = true;

            if (runnerThread != null)
                runnerThread.Abort();

            if (client != null)
            {
                if (client.Connected)
                    client.Shutdown(SocketShutdown.Both);

                client.Close(1000);
                client = null;
            }

            if (listenerSockets != null && listenerSockets.Count > 0)
            {
                foreach (var socket in listenerSockets)
                    socket.Close();

                listenerSockets.Clear();
                listenerSockets = null;
            }
        }

        public int Receive(byte[] data)
        {
            throw new NotImplementedException();
        }

        public int Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<int> AsyncReceive(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<int> AsyncSend(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
