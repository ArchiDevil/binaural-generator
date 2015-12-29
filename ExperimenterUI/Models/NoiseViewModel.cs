using NetworkLayer.Protocol;

namespace ExperimenterUI.Models
{
    public class NoiseViewModel : BaseSignalModel
    {
        private readonly double _maxSmoothness = 0.999;
        private readonly double _minSmoothness = 0.9;
        private double _smoothness = 0.95;
        private readonly double _smoothnessStep = 0.001;

        public double MaxSmoothness
        {
            get { return _maxSmoothness; }
        }

        public double MinSmoothness
        {
            get { return _minSmoothness; }
        }

        public double Smoothness
        {
            get { return _smoothness; }
            set { _smoothness = value; RaisePropertyChanged("Smoothness"); }
        }

        public double SmoothnessStep
        {
            get { return _smoothnessStep; }
        }

        public NoiseViewModel(string signalName, ClientProtocol protocol) 
            : base(signalName, protocol, 0.0, 100.0, 2.5)
        {
        }
    }
}
