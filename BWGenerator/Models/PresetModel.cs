using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using SharedContent.Models;

namespace BWGenerator.Models
{
    public class PresetModel : ModelBase
    {
        public class Signal : ModelBase
        {
            public struct SignalPoint
            {
                public double Time { get; set; }
                public double DifferenceValue { get; set; }
                public double CarrierValue { get; set; }
            }

            private string name = "";

            public string Name
            {
                get { return name; }
                set { name = value; RaisePropertyChanged("Name"); }
            }

            public List<SignalPoint> SignalPoints = new List<SignalPoint>();
        }

        private string name = "";
        private string description = "";

        public PresetModel()
        {
            Name = "";
            Description = "";
            Signals = new ObservableCollection<Signal>();
        }

        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged("Name"); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; RaisePropertyChanged("Description"); }
        }

        public TimeSpan TotalLength
        {
            get
            {
                double minTime = 0;
                double maxTime = 0;
                foreach (var signal in Signals)
                {
                    if (signal.SignalPoints[0].Time < minTime)
                        minTime = signal.SignalPoints[0].Time;

                    if (signal.SignalPoints[signal.SignalPoints.Count - 1].Time > maxTime)
                        maxTime = signal.SignalPoints[signal.SignalPoints.Count - 1].Time;
                }
                int secondsTime = (int)(maxTime - minTime);
                return new TimeSpan(secondsTime / 3600, secondsTime / 60 % 60, secondsTime % 60);
            }
        }

        public ObservableCollection<Signal> Signals { get; set; }
    }
}
