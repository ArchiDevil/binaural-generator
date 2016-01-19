using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using ExperimenterUI.Models;
using NetworkLayer.Protocol;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
        private Logger              _logger = new Logger();
        private NoiseViewModel      _noiseModel = null;
        private SignalViewModel[]   _signalModels = null;
        private string[]            _signalModelNames = null;
        private SignalViewModel     _currentSignal = null;
        private TimeSpan            _sessionTime = new TimeSpan(0, 0, 0);
        private string              _subjectName = "";
        private Timer               _tickTimer = new Timer(1000);
        private PlotModel           _pulseModel = new PlotModel();
        private PlotModel           _motionModel = new PlotModel();
        private PlotModel           _resistanceModel = new PlotModel();
        private PlotModel           _temperatureModel = new PlotModel();
        private int                 _timestamp = 0;

        public NoiseViewModel NoiseModel
        {
            get { return _noiseModel; }
        }

        public SignalViewModel[] SignalModels
        {
            get { return _signalModels; }
        }

        public string[] SignalModelNames
        {
            get { return _signalModelNames; }
            private set { _signalModelNames = value; RaisePropertyChanged("SignalModelNames"); }
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

        public SignalViewModel CurrentSignal
        {
            get { return _currentSignal; }
            private set { _currentSignal = value; RaisePropertyChanged("CurrentSignal"); }
        }

        public event ClientProtocol.ChatMessageReceiveHandler ChatMessageReceived = delegate
        { };

        public ExperimenterApplicationModel(ClientProtocol protocol)
        {
            Contract.Requires(protocol != null, "protocol mustn't be null");

            const int signalsCount = 4;
            _noiseModel = new NoiseViewModel("Noise channel", protocol);
            _signalModels = new SignalViewModel[signalsCount];
            _signalModelNames = new string[signalsCount];
            for (int i = 0; i < signalsCount; i++)
            {
                string signalName = "Channel " + (i + 1);
                _signalModels[i] = new SignalViewModel(signalName, protocol);
                _signalModels[i].PropertyChanged += SignalModelPropertyChanged;
                _signalModelNames[i] = signalName;
            }

            _protocol = protocol;
            _protocol.SensorsReceive += _protocol_SensorsReceive;
            _noiseModel.PropertyChanged += NoiseModelPropertyChanged;

            _tickTimer.Elapsed += _tickTimer_Elapsed;
            _tickTimer.AutoReset = true;
            _tickTimer.Start();

            _pulseModel.Title = "Pulse";
            _pulseModel.Axes.Add(new LinearAxis
            {
                Maximum = 120,
                Minimum = 20,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            _pulseModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            _pulseModel.Series.Add(new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
            });

            _motionModel.Title = "Motion";
            _motionModel.Axes.Add(new LinearAxis
            {
                Maximum = 100,
                Minimum = 0,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            _motionModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            _motionModel.Series.Add(new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
            });

            _resistanceModel.Title = "Resistance";
            _resistanceModel.Axes.Add(new LogarithmicAxis
            {
                Maximum = 10e9,
                Minimum = 10e3,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                StringFormat = "0.###E+0",
            });
            _resistanceModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            _resistanceModel.Series.Add(new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
            });

            _temperatureModel.Title = "Temperature";
            _temperatureModel.Axes.Add(new LinearAxis
            {
                Maximum = 37.0,
                Minimum = 35.0,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            _temperatureModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            _temperatureModel.Series.Add(new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
            });
        }

        private void _protocol_SensorsReceive(object sender, SensorsDataEventArgs e)
        {
            _timestamp += 1;
            LineSeries s = _pulseModel.Series[0] as LineSeries;
            s.Points.Add(new DataPoint(_timestamp, e.pulseValue));
            _pulseModel.InvalidatePlot(false);

            s = _motionModel.Series[0] as LineSeries;
            s.Points.Add(new DataPoint(_timestamp, e.motionValue));
            _motionModel.InvalidatePlot(false);

            s = _resistanceModel.Series[0] as LineSeries;
            s.Points.Add(new DataPoint(_timestamp, e.skinResistanceValue));
            _resistanceModel.InvalidatePlot(false);

            s = _temperatureModel.Series[0] as LineSeries;
            s.Points.Add(new DataPoint(_timestamp, e.temperatureValue));
            _temperatureModel.InvalidatePlot(false);

            _logger.LogSensors(new SensorsData
            {
                motionValue = e.motionValue,
                pulseValue = e.pulseValue,
                skinResistanceValue = e.skinResistanceValue,
                temperatureValue = e.temperatureValue
            });
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

            _logger.LogSignalsChange(_signalModels, _noiseModel);
        }

        private void SignalModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SendNewSettings();
        }

        private void NoiseModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SendNewSettings();
        }

        internal async void Connect(string connectionAddress)
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
                    _logger.StartSession();
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

        internal bool SendChatMessage(string messageContent, DateTime messageTime)
        {
            return _protocol.SendChatMessage(messageContent);
        }

        internal void SelectChannel(int selectedIndex)
        {
            CurrentSignal = _signalModels[selectedIndex];
        }

        internal void StartNewSession()
        {
            _logger = new Logger();
        }

        internal void CloseSession(string filename)
        {
            _logger.EndSession();
            _logger.DumpData(filename);
        }

        public void Dispose()
        {
            _logger.EndSession();
            if (_protocol != null)
                _protocol.Dispose();
        }
    }
}
