using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetworkLayer.Protocol;

using SharedLibrary.Models;

namespace ExperimenterUI.Models
{
    public class SignalViewModel : BaseSignalModel
    {
        private readonly double maxFrequency = 800.0;
        private readonly double minFrequency = 100.0;
        private double frequency = 440.0;
        private readonly double frequencyStep = 10.0;

        private readonly double maxDifference = 26.0;
        private readonly double minDifference = 0.5;
        private double difference = 10.0;
        private readonly double differenceStep = 0.5;

        public double MaxFrequency
        {
            get { return maxFrequency; }
        }

        public double MinFrequency
        {
            get { return minFrequency; }
        }

        public double Frequency
        {
            get { return frequency; }
            set { frequency = value; RaisePropertyChanged("Frequency"); }
        }

        public double FrequencyStep
        {
            get { return frequencyStep; }
        }

        public double MaxDifference
        {
            get { return maxDifference; }
        }

        public double MinDifference
        {
            get { return minDifference; }
        }

        public double Difference
        {
            get { return difference; }
            set { difference = value; RaisePropertyChanged("Difference"); }
        }

        public double DifferenceStep
        {
            get { return differenceStep; }
        }

        public SignalViewModel(string signalName, ClientProtocol protocol) : base(signalName, protocol)
        {
        }
    }
}
