using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using AudioCore.Layers;
using ExperimenterUI.Models;
using NetworkLayer;
using NetworkLayer.ProtocolShared;
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
        private ClientAudioLayer    _audioLayer = null;
        private string[]            _signalModelNames = null;
        private SignalViewModel     _currentSignal = null;
        private Timer               _tickTimer = new Timer(1000);
        private int                 _timestamp = 0;

        public NoiseViewModel NoiseModel { get; } = null;
        public SignalViewModel[] SignalModels { get; } = null;

        public string[] SignalModelNames
        {
            get { return _signalModelNames; }
            private set { _signalModelNames = value; RaisePropertyChanged(); }
        }

        public bool IsConnected
        {
            get { return _connectionStatus == ConnectionStatus.Connected; }
        }

        public TimeSpan SessionTime { get; private set; } = new TimeSpan(0, 0, 0);
        public string SubjectName { get; } = "";
        public PlotModel PulseModel { get; } = new PlotModel();
        public PlotModel MotionModel { get; } = new PlotModel();
        public PlotModel ResistanceModel { get; } = new PlotModel();
        public PlotModel TemperatureModel { get; } = new PlotModel();

        public SignalViewModel CurrentSignal
        {
            get { return _currentSignal; }
            private set { _currentSignal = value; RaisePropertyChanged(); }
        }

        public event ClientProtocol.ChatMessageReceiveHandler ChatMessageReceived = delegate { };

        public ExperimenterApplicationModel(ClientProtocol protocol)
        {
            const int signalsCount = 4;
            NoiseModel = new NoiseViewModel("Noise channel", protocol);
            SignalModels = new SignalViewModel[signalsCount];
            _signalModelNames = new string[signalsCount];
            for (int i = 0; i < signalsCount; i++)
            {
                string signalName = "Channel " + (i + 1);
                SignalModels[i] = new SignalViewModel(signalName, protocol);
                SignalModels[i].PropertyChanged += SignalModelPropertyChanged;
                _signalModelNames[i] = signalName;
            }

            _protocol = protocol ?? throw new ArgumentNullException("protocol");
            _protocol.SensorsReceived += _protocol_SensorsReceive;
            _protocol.ConnectionFinished += _protocol_ConnectionFinished;
            _protocol.ConnectionLost += _protocol_ConnectionLost;

            NoiseModel.PropertyChanged += NoiseModelPropertyChanged;
            _audioLayer = new ClientAudioLayer(_protocol)
            {
                PlaybackEnabled = false
            };

            _tickTimer.Elapsed += _tickTimer_Elapsed;
            _tickTimer.AutoReset = true;
            _tickTimer.Start();

            PulseModel.Title = "Pulse";
            PulseModel.Axes.Add(new LinearAxis
            {
                Maximum = 120,
                Minimum = 20,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            PulseModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            PulseModel.Series.Add(new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
            });

            MotionModel.Title = "Motion";
            MotionModel.Axes.Add(new LinearAxis
            {
                Maximum = 100,
                Minimum = 0,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            MotionModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            MotionModel.Series.Add(new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
            });

            ResistanceModel.Title = "Resistance";
            ResistanceModel.Axes.Add(new LogarithmicAxis
            {
                Maximum = 10e9,
                Minimum = 10e3,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                StringFormat = "0.###E+0",
            });
            ResistanceModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            ResistanceModel.Series.Add(new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
            });

            TemperatureModel.Title = "Temperature";
            TemperatureModel.Axes.Add(new LinearAxis
            {
                Maximum = 37.0,
                Minimum = 35.0,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            TemperatureModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });
            TemperatureModel.Series.Add(new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
            });
        }

        private void _protocol_ConnectionLost(object sender, EventArgs e)
        {
            _connectionStatus = ConnectionStatus.Error;
            RaisePropertyChanged("IsConnected");
        }

        private void _protocol_ConnectionFinished(object sender, EventArgs e)
        {
            _connectionStatus = ConnectionStatus.NoConnection;
            RaisePropertyChanged("IsConnected");
        }

        private void _protocol_SensorsReceive(object sender, SensorsDataEventArgs e)
        {
            _timestamp += 1;
            LineSeries s = PulseModel.Series[0] as LineSeries;
            s.Points.Add(new DataPoint(_timestamp, e.pulseValue));
            PulseModel.InvalidatePlot(false);

            s = MotionModel.Series[0] as LineSeries;
            s.Points.Add(new DataPoint(_timestamp, e.motionValue));
            MotionModel.InvalidatePlot(false);

            s = ResistanceModel.Series[0] as LineSeries;
            s.Points.Add(new DataPoint(_timestamp, e.skinResistanceValue));
            ResistanceModel.InvalidatePlot(false);

            s = TemperatureModel.Series[0] as LineSeries;
            s.Points.Add(new DataPoint(_timestamp, e.temperatureValue));
            TemperatureModel.InvalidatePlot(false);

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
                SessionTime += new TimeSpan(0, 0, 1);
                RaisePropertyChanged("SessionTime");
            }
        }

        private void SendNewSettings()
        {
            ChannelDescription[] channelDescs = new ChannelDescription[SignalModels.Length];
            for (int i = 0; i < channelDescs.Length; ++i)
            {
                channelDescs[i].carrierFrequency = SignalModels[i].Frequency;
                channelDescs[i].differenceFrequency = SignalModels[i].Difference;
                channelDescs[i].volume = SignalModels[i].Gain;
                channelDescs[i].enabled = SignalModels[i].Enabled;
            }

            NoiseDescription noiseDesc = new NoiseDescription()
            {
                smoothness = NoiseModel.Smoothness,
                volume = NoiseModel.Enabled ? NoiseModel.Gain : 0.0
            };
            _protocol.SendSignalSettings(channelDescs, noiseDesc);
            // _audioLayer.SetSignalSettings(_signalModels, _noiseModel);
            _logger.LogSignalsChange(SignalModels, NoiseModel);
        }

        private void SignalModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SendNewSettings();
        }

        private void NoiseModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SendNewSettings();
        }

        internal async void ConnectAsync(string connectionAddress)
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
                    SessionTime = new TimeSpan(0, 0, 0);
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
            CurrentSignal = SignalModels[selectedIndex];
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
        }
    }
}
