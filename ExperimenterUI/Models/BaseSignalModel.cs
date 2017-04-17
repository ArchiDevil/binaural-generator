using System;
using NetworkLayer;
using SharedLibrary.Models;

namespace ExperimenterUI.Models
{
    public class BaseSignalModel : ModelBase
    {
        private bool _enabled = false;
        private double _gain = 50.0;

        protected readonly ClientProtocol _protocol = null;

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; RaisePropertyChanged(); }
        }

        public double MaxGain { get; } = 100.0;
        public double MinGain { get; } = 0.0;
        public double GainStep { get; } = 1.0;
        public string SignalName { get; } = "";

        public double Gain
        {
            get { return _gain; }
            set { _gain = value; RaisePropertyChanged(); }
        }

        public BaseSignalModel(string signalName, ClientProtocol protocol)
        {
            SignalName = signalName;
            _protocol = protocol ?? throw new ArgumentNullException("protocol");
        }

        public BaseSignalModel(string signalName, ClientProtocol protocol, double minGain, double maxGain, double gainStep)
            : this(signalName, protocol)
        {
            MinGain = minGain;
            MaxGain = maxGain;
            Gain = maxGain;
            GainStep = gainStep;
        }
    }
}
