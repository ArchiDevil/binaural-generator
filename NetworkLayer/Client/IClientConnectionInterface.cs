using System.Threading.Tasks;

namespace NetworkLayer
{
    /// <summary>
    /// This interface is used in client implementations.
    /// In our terminology, client is the experimenter.
    /// </summary>
    public interface IClientConnectionInterface
    {
        /// <summary>
        /// This method blocks execution until client connects to the server or connection will fail.
        /// </summary>
        /// <param name="address">Address to connect</param>
        /// <param name="port">Port to connect</param>
        /// <returns>true if connection completed successfully, false otherwise</returns>
        bool Connect(string address, int port);

        /// <summary>
        /// Shuts down client and breaks connection with the server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends data to the server. Blocks execution until data will be sent.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <returns>count of sent bytes, -1 on error</returns>
        int Send(byte[] data);

        /// <summary>
        /// Receives data from the server. Blocks execution until some data will be received.
        /// </summary>
        /// <param name="data">Buffer where to place received data</param>
        /// <returns>count of received bytes</returns>
        int Receive(byte[] data);

        /// <summary>
        /// Receives data from the server. Blocks on timeout milliseconds.
        /// </summary>
        /// <param name="data">Buffer where to place received data</param>
        /// <param name="millisecondsTimeout">Timeout to receive in milliseconds</param>
        /// <returns>count of received bytes, -1 on error</returns>
        int Receive(byte[] data, int millisecondsTimeout);

        /// <summary>
        /// Receives data from the server. Blocks execution until some data will be received.
        /// </summary>
        /// <param name="data">Buffer where to place received data</param>
        /// <param name="offset">Offset in receiving buffer</param>
        /// <param name="size">Size of receiving buffer</param>
        /// <returns>count of sent bytes, -1 on error</returns>
        int Receive(byte[] data, int offset, int size);
    }
}
