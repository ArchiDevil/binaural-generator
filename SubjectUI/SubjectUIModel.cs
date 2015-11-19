using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedLibrary.Models;

namespace SubjectUI
{
    public class SubjectUIModel : ModelBase
    {
        private bool _connectionStatus = false;
        private bool _enableMicrophone = true;
        private bool _enableVoice = true;
        private bool _enableSignals = true;
        private bool _sensorsStatus = false;
        private bool _headphonesStatus = false;
        private bool _voiceStatus = false;
        private bool _chatStatus = false;

        public string ConnectionStatus
        {
            get { return _connectionStatus ? "Connected" : "No connection"; }
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
        public bool SensorsStatus
        {
            get { return _sensorsStatus; }
        }
        public bool HeadphonesStatus
        {
            get { return _headphonesStatus; }
        }
        public bool VoiceStatus
        {
            get { return _voiceStatus; }
        }
        public bool ChatStatus
        {
            get { return _chatStatus; }
        }

        public SubjectUIModel()
        {
            // stub for now
        }

        public bool SendChatMessage(string messageContent, DateTime messageTime)
        {
            // stub for now
            return false;
        }

        public void CheckSensorsStatus()
        {
            // stub for now
            _sensorsStatus = false;
            RaisePropertyChanged("SensorsStatus");
        }

        public void CheckHeadphonesStatus()
        {
            // stub for now
            _headphonesStatus = false;
            RaisePropertyChanged("HeadphonesStatus");
        }

        public void CheckVoiceStatus()
        {
            // stub for now
            _voiceStatus = false;
            RaisePropertyChanged("VoiceStatus");
        }

        public void CheckChatStatus()
        {
            // stub for now
            _chatStatus = false;
            RaisePropertyChanged("ChatStatus");
        }
    }
}
