using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

using ExperimenterUI.Models;

using NetworkLayer.Protocol;

using SharedLibrary.Models;

namespace ExperimenterUI
{
    public sealed class ExperimenterApplicationModel : ModelBase, IDisposable
    {
        private enum ConnectionStatus
        {
            NoConnection,
            Connection,
            Connected,
            Error
        }

        private ConnectionStatus _connectionStatus = ConnectionStatus.NoConnection;
        private string _connectionErrorMessage = "";
        private ClientProtocol protocol = null;
        private NoiseViewModel noiseModel = null;
        private SignalViewModel[] signalModels = null;
        private TimeSpan _sessionTime = new TimeSpan(0, 0, 0);
        private Timer _tickTimer = null;

        public NoiseViewModel NoiseModel
        {
            get { return noiseModel; }
        }

        public SignalViewModel[] SignalModels
        {
            get { return signalModels; }
        }

        public bool IsConnected
        {
            get { return _connectionStatus == ConnectionStatus.Connected; }
        }

        public TimeSpan SessionTime
        {
            get { return _sessionTime; }
        }

        public event ClientProtocol.ChatMessageReceiveHandler ChatMessageReceived = delegate
        { };

        public ExperimenterApplicationModel(ClientProtocol protocol)
        {
            if (protocol == null)
                throw new ArgumentNullException("protocol");

            noiseModel = new NoiseViewModel("Noise channel", protocol);

            signalModels = new SignalViewModel[4];
            for (int i = 0; i < signalModels.Length; i++)
            {
                signalModels[i] = new SignalViewModel("Channel " + (i + 1).ToString(), protocol);
                signalModels[i].PropertyChanged += SignalModelPropertyChanged;
            }

            this.protocol = protocol;
            noiseModel.PropertyChanged += NoiseModelPropertyChanged;

            _tickTimer = new Timer(1000);
            _tickTimer.Elapsed += _tickTimer_Elapsed;
            _tickTimer.AutoReset = true;
            _tickTimer.Start();
        }

        private void _tickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_connectionStatus == ConnectionStatus.Connected)
            {
                _sessionTime += new TimeSpan(0, 0, 1);
                RaisePropertyChanged("SessionTime");
            }
        }

        private void SendNewSettings()
        {
            ChannelDescription[] channelDescs = new ChannelDescription[signalModels.Length];
            for (int i = 0; i < channelDescs.Length; ++i)
            {
                channelDescs[i].carrierFrequency = signalModels[i].Frequency;
                channelDescs[i].differenceFrequency = signalModels[i].Difference;
                channelDescs[i].volume = signalModels[i].Gain;
                channelDescs[i].enabled = signalModels[i].Enabled;
            }

            NoiseDescription noiseDesc = new NoiseDescription();
            noiseDesc.smoothness = noiseModel.Smoothness;
            noiseDesc.volume = noiseModel.Gain;

            protocol.SendSignalSettings(channelDescs, noiseDesc);
        }

        private void SignalModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SendNewSettings();
        }

        private void NoiseModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SendNewSettings();
        }

        public async void Connect(string connectionAddress)
        {
            _connectionStatus = ConnectionStatus.Connection;
            RaisePropertyChanged("ConnectionStatus");
            RaisePropertyChanged("IsConnected");

            Task<bool> t = new Task<bool>(() => protocol.Connect(connectionAddress));
            t.Start();
            try
            {
                bool status = await t;
                if (!status)
                {
                    _connectionStatus = ConnectionStatus.Error;
                    RaisePropertyChanged("ConnectionStatus");
                    RaisePropertyChanged("IsConnected");
                }
                else
                {
                    _connectionStatus = ConnectionStatus.Connected;
                    RaisePropertyChanged("ConnectionStatus");
                    RaisePropertyChanged("IsConnected");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                _connectionErrorMessage = e.Message;

                _connectionStatus = ConnectionStatus.Error;
                RaisePropertyChanged("ConnectionStatus");
                RaisePropertyChanged("IsConnected");
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
