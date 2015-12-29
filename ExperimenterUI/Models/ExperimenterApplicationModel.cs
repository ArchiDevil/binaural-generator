using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using ExperimenterUI.Models;
using NetworkLayer.Protocol;
using OxyPlot;
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

        private ConnectionStatus    _connectionStatus = ConnectionStatus.NoConnection;
        private string              _connectionErrorMessage = "";
        private ClientProtocol      _protocol = null;
        private NoiseViewModel      _noiseModel = null;
        private SignalViewModel[]   _signalModels = null;
        private TimeSpan            _sessionTime = new TimeSpan(0, 0, 0);
        private string              _subjectName = "";
        private Timer               _tickTimer = new Timer(1000);
        private PlotModel           _pulseModel = new PlotModel();
        private PlotModel           _motionModel = new PlotModel();
        private PlotModel           _resistanceModel = new PlotModel();
        private PlotModel           _temperatureModel = new PlotModel();

        public NoiseViewModel NoiseModel
        {
            get { return _noiseModel; }
        }

        public SignalViewModel[] SignalModels
        {
            get { return _signalModels; }
        }

        public bool IsConnected
        {
            get { return _connectionStatus == ConnectionStatus.Connected; }
        }

        public TimeSpan SessionTime
        {
            get { return _sessionTime; }
        }

        public string SubjectName
        {
            get { return _subjectName; }
        }

        public PlotModel PulseModel
        {
            get { return _pulseModel; }
        }

        public PlotModel MotionModel
        {
            get { return _motionModel; }
        }

        public PlotModel ResistanceModel
        {
            get { return _resistanceModel; }
        }

        public PlotModel TemperatureModel
        {
            get { return _temperatureModel; }
        }

        public event ClientProtocol.ChatMessageReceiveHandler ChatMessageReceived = delegate
        { };

        public ExperimenterApplicationModel(ClientProtocol protocol)
        {
            Contract.Requires(protocol != null, "protocol mustn't be null");

            _noiseModel = new NoiseViewModel("Noise channel", protocol);
            _signalModels = new SignalViewModel[4];
            for (int i = 0; i < _signalModels.Length; i++)
            {
                _signalModels[i] = new SignalViewModel("Channel " + (i + 1).ToString(), protocol);
                _signalModels[i].PropertyChanged += SignalModelPropertyChanged;
            }

            _protocol = protocol;
            _noiseModel.PropertyChanged += NoiseModelPropertyChanged;

            _tickTimer.Elapsed += _tickTimer_Elapsed;
            _tickTimer.AutoReset = true;
            _tickTimer.Start();

            _pulseModel.Title = "Pulse value";
            _motionModel.Title = "Motion value";
            _resistanceModel.Title = "Resistance value";
            _temperatureModel.Title = "Temperature value";
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
            ChannelDescription[] channelDescs = new ChannelDescription[_signalModels.Length];
            for (int i = 0; i < channelDescs.Length; ++i)
            {
                channelDescs[i].carrierFrequency = _signalModels[i].Frequency;
                channelDescs[i].differenceFrequency = _signalModels[i].Difference;
                channelDescs[i].volume = _signalModels[i].Gain;
                channelDescs[i].enabled = _signalModels[i].Enabled;
            }

            NoiseDescription noiseDesc = new NoiseDescription();
            noiseDesc.smoothness = _noiseModel.Smoothness;
            noiseDesc.volume = _noiseModel.Enabled ? _noiseModel.Gain : 0.0;

            _protocol.SendSignalSettings(channelDescs, noiseDesc);
        }

        private void SignalModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SendNewSettings();
        }

        private void NoiseModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SendNewSettings();
        }

        public async void Connect(string connectionAddress)
        {
            _connectionStatus = ConnectionStatus.Connection;
            RaisePropertyChanged("IsConnected");

            Task<bool> t = new Task<bool>(() => _protocol.Connect(connectionAddress));
            t.Start();
            try
            {
                bool status = await t;
                if (!status)
                {
                    _connectionStatus = ConnectionStatus.Error;
                    RaisePropertyChanged("IsConnected");
                }
                else
                {
                    _connectionStatus = ConnectionStatus.Connected;
                    RaisePropertyChanged("IsConnected");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                _connectionErrorMessage = e.Message;

                _connectionStatus = ConnectionStatus.Error;
                RaisePropertyChanged("IsConnected");
            }
        }

        public bool SendChatMessage(string messageContent, DateTime messageTime)
        {
            return _protocol.SendChatMessage(messageContent);
        }

        public void Dispose()
        {
            if (_protocol != null)
                _protocol.Dispose();
        }
    }
}
