using System.ComponentModel;
using System.Runtime.CompilerServices;

using AudioCore.AudioPrimitives;

namespace BWSitterGenerator.Models
{
    class NoiseModel : BasicNoiseModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public new bool Enabled
        {
            get { return base.Enabled; }
            set { base.Enabled = value; RaisePropertyChanged(); }
        }

        public new double Gain
        {
            get { return base.Gain; }
            set { base.Gain = value; RaisePropertyChanged(); }
        }

        public new double Smoothness
        {
            get { return base.Smoothness; }
            set { base.Smoothness = value; RaisePropertyChanged(); }
        }
    }
}
