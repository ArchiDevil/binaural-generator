using NetworkLayer;

namespace ExperimenterUI.Models
{
    public class SignalViewModel : BaseSignalModel
    {
        private double _frequency = 440.0;
        private double _difference = 10.0;

        public double MaxFrequency { get; } = 800.0;
        public double MinFrequency { get; } = 100.0;
        public double FrequencyStep { get; } = 10.0;

        public double MaxDifference { get; } = 26.0;
        public double MinDifference { get; } = 0.5;
        public double DifferenceStep { get; } = 0.5;

        public double Frequency
        {
            get { return _frequency; }
            set { _frequency = value; RaisePropertyChanged("Frequency"); }
        }

        public double Difference
        {
            get { return _difference; }
            set { _difference = value; RaisePropertyChanged("Difference"); }
        }

        public SignalViewModel(string signalName, ClientProtocol protocol) 
            : base(signalName, protocol, 0.0, 20.0, 2.0)
        {
        }
    }
}
