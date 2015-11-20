using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public SubjectUIModel()
        {
            // stub for now
        }

        public bool SendChatMessage(string messageContent, DateTime messageTime)
        {
            // stub for now
            return false;
        }
    }
}
