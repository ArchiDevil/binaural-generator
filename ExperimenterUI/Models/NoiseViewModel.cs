using NetworkLayer.Protocol;

namespace ExperimenterUI.Models
{
    public class NoiseViewModel : BaseSignalModel
    {
        private readonly double maxSmoothness = 0.999;
        private readonly double minSmoothness = 0.9;
        private double smoothness = 0.95;
        private readonly double smoothnessStep = 0.001;

        public double MaxSmoothness
        {
            get { return maxSmoothness; }
        }

        public double MinSmoothness
        {
            get { return minSmoothness; }
        }

        public double Smoothness
        {
            get { return smoothness; }
            set { smoothness = value; RaisePropertyChanged("Smoothness"); }
        }

        public double SmoothnessStep
        {
            get { return smoothnessStep; }
        }

        public NoiseViewModel(string signalName, ClientProtocol protocol) : base(signalName, protocol)
        {
        }
    }
}
