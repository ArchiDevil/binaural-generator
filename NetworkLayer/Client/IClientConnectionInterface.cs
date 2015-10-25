using System.Threading.Tasks;

namespace NetworkLayer
{
    public interface IClientConnectionInterface
    {
        bool Connect(string address, int port);
        void Disconnect();

        int Send(byte[] data);
        int Receive(byte[] data);

        Task<int> AsyncSend(byte[] data);
        Task<int> AsyncReceive(byte[] data);
    }
}
