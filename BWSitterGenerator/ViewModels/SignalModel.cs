using SharedContent.Models;

namespace BWSitterGenerator.Models
{
    class SignalModel : BasicSignalModel
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

        public float Frequency
        {
            get { return frequency; }
            set { frequency = value; RaisePropertyChanged("Frequency"); }
        }

        public float Difference
        {
            get { return difference; }
            set { difference = value; RaisePropertyChanged("Difference"); }
        }
    }
}
