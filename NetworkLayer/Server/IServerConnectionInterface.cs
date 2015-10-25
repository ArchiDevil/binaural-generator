using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLayer
{
    public interface IServerConnectionInterface
    {
        bool StartListening(int port);
        bool StartListening(string bindPoint, int port);
        void Shutdown();

        int Send(byte[] data);
        int Receive(byte[] data);

        Task<int> AsyncSend(byte[] data);
        Task<int> AsyncReceive(byte[] data);
    }
}
