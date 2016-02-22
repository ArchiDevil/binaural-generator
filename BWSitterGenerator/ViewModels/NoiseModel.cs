using System.ComponentModel;

using AudioCore.AudioPrimitives;

namespace BWSitterGenerator.Models
{
    class NoiseModel : BasicNoiseModel
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

        public double Smoothness
        {
            get { return smoothness; }
            set { smoothness = value; RaisePropertyChanged("Smoothness"); }
        }
    }
}
