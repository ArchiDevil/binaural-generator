using NetworkLayer.Protocol;

namespace ExperimenterUI.Models
{
    public class SignalViewModel : BaseSignalModel
    {
        private readonly double _maxFrequency = 800.0;
        private readonly double _minFrequency = 100.0;
        private double _frequency = 440.0;
        private readonly double _frequencyStep = 10.0;

        private readonly double _maxDifference = 26.0;
        private readonly double _minDifference = 0.5;
        private double _difference = 10.0;
        private readonly double _differenceStep = 0.5;

        public double MaxFrequency
        {
            get { return _maxFrequency; }
        }

        public double MinFrequency
        {
            get { return _minFrequency; }
        }

        public double Frequency
        {
            get { return _frequency; }
            set { _frequency = value; RaisePropertyChanged("Frequency"); }
        }

        public double FrequencyStep
        {
            get { return _frequencyStep; }
        }

        public double MaxDifference
        {
            get { return _maxDifference; }
        }

        public double MinDifference
        {
            get { return _minDifference; }
        }

        public double Difference
        {
            get { return _difference; }
            set { _difference = value; RaisePropertyChanged("Difference"); }
        }

        public double DifferenceStep
        {
            get { return _differenceStep; }
        }

        public SignalViewModel(string signalName, ClientProtocol protocol) 
            : base(signalName, protocol, 0.0, 20.0, 2.0)
        {
        }
    }
}
