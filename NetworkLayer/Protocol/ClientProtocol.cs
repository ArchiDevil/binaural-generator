using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLayer.Protocol
{
    public class ClientProtocol
    {
        public delegate void SensorsReceiveHandler(object sender, SensorsDataEventArgs e);
        public delegate void VoiceWindowReceiveHandler(object sender, VoiceWindowDataEventArgs e);
        public delegate void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e);

        IClientConnectionInterface connectionInterface = null;

        public ClientProtocol(string clientName)
        {
        }

        public bool Connect(string address)
        {
            return false;
        }

        public void Disconnect()
        {
        }

        public bool SendSignalSettings(SensorsDataEventArgs data)
        {
            return false;
        }

        public bool SendVoiceWindow(VoiceWindowDataEventArgs data)
        {
            return false;
        }

        public bool SendChatMessage(string message)
        {
            return false;
        }

        public event SensorsReceiveHandler SensorsReceive = delegate
        { };
        public event VoiceWindowReceiveHandler VoiceWindowReceive = delegate
        { };
        public event ChatMessageReceiveHandler ChatMessageReceive = delegate
        { };
    }
}
