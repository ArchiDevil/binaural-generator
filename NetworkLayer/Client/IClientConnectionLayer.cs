using NetworkLayer.ConnectionLayerShared;

namespace NetworkLayer.Client
{
    public delegate void ClientConnectionHandler(object sender);
    public delegate void ClientDisconnectionHandler(object sender);
    public delegate void PacketReceivedHander(object sender, PacketReceivedEventArgs e);

    interface IClientConnectionLayer
    {
        bool Connect(string address, ushort port);
        bool Disconnect();
        bool SendData(byte[] data);

        bool IsConnected();

        event ClientConnectionHandler ConnectionEstablished;
        event ClientDisconnectionHandler ConnectionLost;
        event ClientDisconnectionHandler ConnectionFinished;
        event PacketReceivedHander PacketReceived;
    }
}
