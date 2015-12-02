using System;
using NetworkLayer.Protocol;
using SharedLibrary.Models;

namespace ExperimenterUI.Models
{
    public class BaseSignalModel : ModelBase
    {
        private readonly string signalName = "";
        private bool enabled = false;

        private readonly double maxGain = 100.0;
        private readonly double minGain = 0.0;
        private double gain = 50.0;

        protected readonly ClientProtocol protocol = null;

        public string SignalName
        {
            get { return signalName; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                RaisePropertyChanged("Enabled");
            }
        }

        public double MaxGain
        {
            get { return maxGain; }
        }

        public double MinGain
        {
            get { return minGain; }
        }

        public double Gain
        {
            get { return gain; }
            set { gain = value; RaisePropertyChanged("Gain"); }
        }

        public BaseSignalModel(string signalName, ClientProtocol protocol)
        {
            this.signalName = signalName;

            if (protocol == null)
                throw new ArgumentNullException("protocol");
            this.protocol = protocol;
        }
    }
}
