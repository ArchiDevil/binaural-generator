using SharedLibrary.Models;

namespace BWSitterGenerator.Models
{
    class NoiseModel : BasicNoiseModel
    {
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; RaisePropertyChanged("Enabled"); }
        }

        public float Gain
        {
            get { return gain; }
            set { gain = value; RaisePropertyChanged("Gain"); }
        }

        public double Smoothness
        {
            get { return smoothness; }
            set { smoothness = value; RaisePropertyChanged("Smoothness"); }
        }
    }
}
