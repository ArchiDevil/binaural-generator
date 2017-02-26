using System;
using AudioCore.Layers;
using NetworkLayer;
using NetworkLayer.ProtocolShared;
using SensorsLayer;
using SharedLibrary.Models;

namespace SubjectUI
{
    public sealed class SubjectApplicationModel : ModelBase
    {
        private bool _connectionStatus = false;
        private bool _enableVoice = true;
        private bool _enableSignals = true;

        private bool _areSensorsEnabled = false;
        private bool _isMicrophoneEnabled = false;

        private string _sensorsDeviceStatus = "";

        private ServerProtocol _protocol = new ServerProtocol("Subject");
        private SensorsCollector _collector = new SensorsCollector();
        private ServerAudioLayer _audioLayer = null;

        public delegate void ChatMessageReceiveHandler(string message, DateTime time);
        public event ChatMessageReceiveHandler ChatMessageReceivedEvent = delegate { };

        public string ConnectionStatus
        {
            get { return _connectionStatus ? "Connected" : "Waiting connection"; }
        }

        public bool IsConnected
        {
            get { return _connectionStatus; }
            private set { _connectionStatus = value; RaisePropertyChanged("IsConnected"); RaisePropertyChanged("ConnectionStatus"); }
        }

        public bool EnableMicrophone
        {
            get { return _audioLayer.RecordingEnabled; }
            set { _audioLayer.RecordingEnabled = value; RaisePropertyChanged("EnableMicrophone"); }
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
            private set { _areSensorsEnabled = value; RaisePropertyChanged("AreSensorsEnabled"); }
        }

        public bool IsMicrophoneEnabled
        {
            get { return _isMicrophoneEnabled; }
            private set { _isMicrophoneEnabled = value; RaisePropertyChanged("IsMicrophoneEnabled"); }
        }

        public string SensorsDeviceStatus
        {
            get { return _sensorsDeviceStatus; }
            private set { _sensorsDeviceStatus = value; RaisePropertyChanged("SensorsDeviceStatus"); }
        }

        public SubjectApplicationModel()
        {
            if (!_protocol.Bind())
                throw new Exception("Unable to start server");

            _protocol.ClientConnected += ClientConnected;
            _protocol.ChatMessageReceived += ChatMessageReceived;

            _audioLayer = new ServerAudioLayer(_protocol)
            {
                PlaybackEnabled = true,
                RecordingEnabled = true
            };
            IsMicrophoneEnabled = _audioLayer.AudioInDevicesCount > 0;

            SensorsDeviceStatus = "Device disconnected";

            // start checking microphone and sensors
            _collector.SensorsDataReceived += SensorsDataReceived;
            _collector.DeviceConnected += SensorsDeviceConnected;
            _collector.DeviceDisconnected += SensorsDeviceDisconnected;
            _collector.StartDeviceExploringAsync();
        }

        private void SensorsDeviceConnected(object sender, ConnectedEventArgs e)
        {
            AreSensorsEnabled = true;
            SensorsDeviceStatus = e.portName.Length > 0 ? "Device connected on port: " + e.portName : "Device connected";
        }

        private void SensorsDeviceDisconnected(object sender, DisconnectedEventArgs e)
        {
            AreSensorsEnabled = false;
            SensorsDeviceStatus = "Device disconnected";
        }

        private void ClientConnected(object sender, ClientInfoEventArgs e)
        {
            IsConnected = true;
        }

        private void ChatMessageReceived(object sender, ClientChatMessageEventArgs e)
        {
            ChatMessageReceivedEvent(e.message, e.sentTime);
        }

        private void SensorsDataReceived(SensorsLayer.SensorsDataEventArgs e)
        {
            if (!AreSensorsEnabled)
                return;

            if (!_protocol.SendSensorsData(e.temperatureValue, e.skinResistanceValue, e.motionValue, e.pulseValue))
            {
                //UNDONE: temporary here before correct error handling will be implemented
                throw new Exception("Unable to send data");
            }
        }

        public bool SendChatMessage(string messageContent, DateTime messageTime)
        {
            return _protocol.SendChatMessage(messageContent);
        }

        public void CheckSystems()
        {
        }

        public void ProtocolStop()
        {
            _protocol.Stop();
        }
    }
}
