using System;

namespace NetworkLayer
{
    /// <summary>
    /// Used in ClientConnected event.
    /// </summary>
    /// <param name="sender">Object called this event</param>
    /// <param name="e">Event arguments</param>
    public delegate void ClientConnectedHandler(object sender, EventArgs e);

    /// <summary>
    /// This interface is used in server implementations.
    /// In our terminology, server is the subject.
    /// </summary>
    public interface IServerConnectionInterface
    {
        /// <summary>
        /// Starts listening and accepting connections on all bind points.
        /// When connection is accepting, ClientConnected event called.
        /// This method is not blocking.
        /// </summary>
        /// <param name="port">Listening port</param>
        /// <returns>true if accepting started successfully, false otherwise</returns>
        bool StartListening(ushort port);

        /// <summary>
        /// Starts listening and accepting connections on provided bind point.
        /// When connection is accepting, ClientConnected event called.
        /// This method is not blocking.
        /// </summary>
        /// <param name="bindPoint">Specific binding point</param>
        /// <param name="port">Listening port</param>
        /// <returns>true if accepting started successfully, false otherwise</returns>
        bool StartListening(string bindPoint, ushort port);

        /// <summary>
        /// Shuts down server and breaks all connections with all clients.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Provides information about listening status.
        /// </summary>
        /// <returns>true if server in listening mode, false otherwise</returns>
        bool IsListening();

        /// <summary>
        /// Provides information about connected clients.
        /// </summary>
        /// <returns>true if at least one client connected, false otherwise</returns>
        bool IsClientConnected();

        /// <summary>
        /// Sends buffer to connected clients.
        /// This method is blocking.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <returns>count of sent bytes</returns>
        int Send(byte[] data);

        /// <summary>
        /// Receives data from connected clients.
        /// This method is blocking.
        /// </summary>
        /// <param name="data">Buffer to store received data</param>
        /// <returns>count of received bytes</returns>
        int Receive(byte[] data);

        /// <summary>
        /// Receives data from connected clients with timeout.
        /// Returns with 0 when timeout expired.
        /// This method is not blocking.
        /// </summary>
        /// <param name="data">Buffer to store received data</param>
        /// <param name="millisecondsTimeout">Timeout to receive in milliseconds</param>
        /// <returns>count of received bytes</returns>
        int Receive(byte[] data, int millisecondsTimeout);

        /// <summary>
        /// This event is called when server accepts connection.
        /// </summary>
        event ClientConnectedHandler ClientConnected;
    }
}
