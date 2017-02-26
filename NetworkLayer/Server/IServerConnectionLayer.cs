using System;
using NetworkLayer.ConnectionLayerShared;

namespace NetworkLayer.Server
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public string clientAddress = string.Empty;
    }

    public delegate void ServerClientConnectionHandler(object sender, ClientConnectedEventArgs e);
    public delegate void ServerClientDisconnectionHandler(object sender);
    public delegate void PacketReceivedHander(object sender, PacketReceivedEventArgs e);

    interface IServerConnectionLayer
    {
        bool Bind(ushort port);
        bool Stop();

        bool SendData(byte[] data);

        bool IsListening();
        bool IsClientConnected();

        event ServerClientConnectionHandler ClientConnected;
        event ServerClientDisconnectionHandler ClientDisconnected;
        event ServerClientDisconnectionHandler ConnectionLost;
        event PacketReceivedHander PacketReceived;
    }
}
