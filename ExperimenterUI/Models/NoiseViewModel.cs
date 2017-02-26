using NetworkLayer;

namespace ExperimenterUI.Models
{
    public class NoiseViewModel : BaseSignalModel
    {
        private double _smoothness = 0.95;

        public double MaxSmoothness { get; } = 0.999;
        public double MinSmoothness { get; } = 0.9;
        public double SmoothnessStep { get; } = 0.001;

        public double Smoothness
        {
            get { return _smoothness; }
            set { _smoothness = value; RaisePropertyChanged("Smoothness"); }
        }

        public NoiseViewModel(string signalName, ClientProtocol protocol) 
            : base(signalName, protocol, 0.0, 100.0, 2.5)
        {
        }
    }
}
