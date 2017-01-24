using System;
using System.Diagnostics.Contracts;
using NetworkLayer.Protocol;
using SharedLibrary.Models;

namespace ExperimenterUI.Models
{
    public class BaseSignalModel : ModelBase
    {
        private readonly string _signalName = "";
        private bool _enabled = false;

        private readonly double _maxGain = 100.0;
        private readonly double _minGain = 0.0;
        private readonly double _gainStep = 1.0;
        private double _gain = 50.0;

        protected readonly ClientProtocol _protocol = null;

        public string SignalName
        {
            get { return _signalName; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; RaisePropertyChanged("Enabled"); }
        }

        public double MaxGain
        {
            get { return _maxGain; }
        }

        public double MinGain
        {
            get { return _minGain; }
        }

        public double Gain
        {
            get { return _gain; }
            set { _gain = value; RaisePropertyChanged("Gain"); }
        }

        public double GainStep
        {
            get { return _gainStep; }
        }

        public BaseSignalModel(string signalName, ClientProtocol protocol)
        {
            Contract.Requires<ArgumentNullException>(protocol != null, "protocol mustn't be null");

            _signalName = signalName;
            _protocol = protocol;
        }

        public BaseSignalModel(string signalName, ClientProtocol protocol, double minGain, double maxGain, double gainStep)
            : this(signalName, protocol)
        {
            _minGain = minGain;
            _maxGain = maxGain;
            Gain = maxGain;
            _gainStep = gainStep;
        }
    }
}
