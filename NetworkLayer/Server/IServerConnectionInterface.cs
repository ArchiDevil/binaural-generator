using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLayer
{
    public delegate void ClientConnectedHandler();

    public interface IServerConnectionInterface
    {
        bool StartListening(int port);
        bool StartListening(string bindPoint, int port);
        void Shutdown();

        bool IsListening();
        bool IsClientConnected();

        int Send(byte[] data);
        int Receive(byte[] data);
        int Receive(byte[] data, int timeout);

        event ClientConnectedHandler ClientConnected;
    }
}
