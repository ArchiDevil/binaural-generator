﻿using System;
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
    public static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
    }

    public class InternetServerConnectionInterface : IServerConnectionInterface
    {
        string bindingPoint = string.Empty;
        ushort port = 0;
        List<Socket> listenerSockets = null;
        Socket client = null;

        public event ClientConnectedHandler ClientConnected = delegate
        { };

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }

            client = e.AcceptSocket;

            if (!client.Connected)
                return;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += AcceptCompleted;
            (sender as Socket).AcceptAsync(args);

            try
            {
                byte[] buffer = new byte[1024];
                int received = client.Receive(buffer);
                string message = Encoding.ASCII.GetString(buffer, 0, received);
                if (message.IndexOf("<EOF>") == -1)
                {
                    throw new Exception("Wrong startup message");
                }

                buffer = Encoding.ASCII.GetBytes("<EOF>");
                int sent = client.Send(buffer);
                if (sent == 0)
                {
                    throw new Exception("Wrong startup message");
                }

                ClientConnected(this, new EventArgs());
            }
            catch (Exception exc)
            {
                Debug.Assert(false, exc.Message);
                // unrecoverable error =(
                Shutdown();
            }
        }

        public bool StartListening(ushort port)
        {
            return StartListening("", port);
        }

        public bool StartListening(string bindingPoint, ushort port)
        {
            Shutdown();
            this.bindingPoint = bindingPoint;
            this.port = port;

            IPHostEntry ipHostInfo = null;
            if (bindingPoint.Length != 0)
                ipHostInfo = Dns.GetHostEntry(bindingPoint);
            else
                ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            if (listenerSockets == null)
                listenerSockets = new List<Socket>();

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
                    SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                    e.Completed += AcceptCompleted;
                    listenerSocket.AcceptAsync(e);
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
            if (client != null)
            {
                client.Close();
                client = null;
            }

            if (listenerSockets != null)
            {
                foreach (var socket in listenerSockets)
                {
                    socket.Dispose();
                }

                listenerSockets.Clear();
                listenerSockets = null;
            }
        }

        public int Receive(byte[] data)
        {
            if (client == null)
                return 0;

            int count = 0;
            try
            {
                if (client.Poll(-1, SelectMode.SelectRead) && client.IsConnected())
                    count = client.Receive(data);
            }
            catch (SocketException e)
            {
                Debug.Assert(false, e.Message);
                client.Close();
                client = null;
            }
            return count;
        }

        public int Receive(byte[] data, int millisecondsTimeout)
        {
            if (client == null)
                return 0;

            int count = 0;
            try
            {
                if (client.Poll(millisecondsTimeout * 1000, SelectMode.SelectRead) && client.IsConnected())
                    count = client.Receive(data);
            }
            catch (SocketException e)
            {
                Debug.Assert(false, e.Message);
                client.Close();
                client = null;
            }
            return count;
        }

        public int Send(byte[] data)
        {
            if (client == null)
                return 0;

            int count = 0;
            try
            {
                if (client.IsConnected())
                    count = client.Send(data);
            }
            catch (SocketException e)
            {
                Debug.Assert(false, e.Message);
                client.Close();
                client = null;
            }
            return count;
        }

        public bool IsListening()
        {
            if (listenerSockets == null)
                return false;

            return listenerSockets.Count > 0;
        }

        public bool IsClientConnected()
        {
            return client != null;
        }
    }
}
