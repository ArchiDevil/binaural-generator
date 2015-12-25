using System.ComponentModel;

using AudioCore;

namespace BWSitterGenerator.Models
{
    class SignalModel : BasicSignalModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
            {
                e(this, new PropertyChangedEventArgs(propertyName));
            }
        }

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

        public double Frequency
        {
            get { return frequency; }
            set { frequency = value; RaisePropertyChanged("Frequency"); }
        }

        public double Difference
        {
            get { return difference; }
            set { difference = value; RaisePropertyChanged("Difference"); }
        }
    }
}
