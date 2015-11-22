using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NetworkLayer.Protocol;

using SharedLibrary.Models;

namespace SubjectUI
{
    public class SubjectApplicationModel : ModelBase
    {
        private bool _connectionStatus = false;
        private bool _enableMicrophone = true;
        private bool _enableVoice = true;
        private bool _enableSignals = true;

        private bool _areSensorsEnabled = false;
        private bool _isMicrophoneEnabled = false;

        private ServerProtocol protocol = new ServerProtocol("Subject");

        private void ClientConnectionHandler(object sender, ClientInfoEventArgs e)
        {
            _connectionStatus = true;
            RaisePropertyChanged("ConnectionStatus");
            RaisePropertyChanged("IsConnected");
        }

        public string ConnectionStatus
        {
            get { return _connectionStatus ? "Connected" : "Waiting connection"; }
        }

        public bool IsConnected
        {
            get { return _connectionStatus; }
        }

        public bool EnableMicrophone
        {
            get { return _enableMicrophone; }
            set { _enableMicrophone = value; RaisePropertyChanged("EnableMicrophone"); }
        }

        public bool EnableVoice
        {
            get { return _enableVoice; }
            set { _enableVoice = value; RaisePropertyChanged("EnableVoice"); }
        }

        public bool EnableSignals
        {
            get { return _enableSignals; }
            set { _enableSignals = value; RaisePropertyChanged("EnableSignals"); }
        }

        public bool AreSensorsEnabled
        {
            get { return _areSensorsEnabled; }
        }

        public bool IsMicrophoneEnabled
        {
            get { return _isMicrophoneEnabled; }
        }

        public SubjectApplicationModel()
        {
            protocol.Bind();
            protocol.ClientConnected += ClientConnectionHandler;

            // start checking microphone and sensors
        }

        public bool SendChatMessage(string messageContent, DateTime messageTime)
        {
            return protocol.SendChatMessage(messageContent);
        }

        public async void CheckSystems()
        {
            Task<bool> task = new Task<bool>(CheckSensors);
            task.Start();
            _areSensorsEnabled = await task;
            RaisePropertyChanged("AreSensorsEnabled");

            task = new Task<bool>(CheckMicrophone);
            task.Start();
            _isMicrophoneEnabled = await task;
            RaisePropertyChanged("IsMicrophoneEnabled");
        }

        public bool CheckSensors()
        {
            // now this is draft, until sensors subsystem won't be completed
            Thread.Sleep(2000);
            return true;
        }

        public bool CheckMicrophone()
        {
            // now this is draft, until sensors audiocore won't be completed
            Thread.Sleep(2000);
            return true;
        }
    }
}
