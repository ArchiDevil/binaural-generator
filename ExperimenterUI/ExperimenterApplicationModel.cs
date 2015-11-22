using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkLayer.Protocol;
using SharedLibrary.Models;

namespace ExperimenterUI
{
    public sealed class ExperimenterApplicationModel : ModelBase, IDisposable
    {
        private enum ConnStatus
        {
            NoConnection,
            Connection,
            Connected,
            Error
        }

        private ConnStatus _connectionStatus = ConnStatus.NoConnection;

        ClientProtocol protocol = new ClientProtocol("Client");

        public string ConnectionStatus
        {
            get
            {
                switch (_connectionStatus)
                {
                    case ConnStatus.NoConnection:
                        return "No connection";
                    case ConnStatus.Connection:
                        return "Connection...";
                    case ConnStatus.Connected:
                        return "Connected";
                    case ConnStatus.Error:
                        return "Error";
                    default:
                        return "";
                }
            }
        }

        public bool IsConnected
        {
            get { return _connectionStatus == ConnStatus.Connected; }
        }

        public ExperimenterApplicationModel()
        {
        }

        public async void Connect(string connectionAddress)
        {
            _connectionStatus = ConnStatus.Connection;
            RaisePropertyChanged("ConnectionStatus");

            Task<bool> t = new Task<bool>(() => protocol.Connect(connectionAddress));
            t.Start();
            bool status = await t;
            if (!status)
            {
                _connectionStatus = ConnStatus.Error;
                RaisePropertyChanged("ConnectionStatus");
            }
            else
            {
                _connectionStatus = ConnStatus.Connected;
                RaisePropertyChanged("ConnectionStatus");
            }
        }

        public bool SendChatMessage(string messageContent, DateTime messageTime)
        {
            return protocol.SendChatMessage(messageContent);
        }

        public void Dispose()
        {
            if (protocol != null)
                protocol.Dispose();
        }
    }
}
