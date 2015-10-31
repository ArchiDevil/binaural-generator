using System.Threading.Tasks;

namespace NetworkLayer
{
    public interface IClientConnectionInterface
    {
        bool Connect(string address, int port);
        void Disconnect();

        int Send(byte[] data);
        int Receive(byte[] data);
        int Receive(byte[] data, int offset, int size);
    }
}
