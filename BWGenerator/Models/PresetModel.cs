using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using SharedLibrary.Models;

namespace BWGenerator.Models
{
    public class SignalPoint
    {
        public double Time { get; set; }
        public double DifferenceValue { get; set; }
        public double CarrierValue { get; set; }
        public double VolumeValue { get; set; }
    }

    public class Signal : ModelBase
    {
        public Signal()
        {
            points = new SignalPoint[2];
            points[0] = new SignalPoint { Time = 0.0, DifferenceValue = 8.0, CarrierValue = 330.0, VolumeValue = 80.0 };
            points[1] = new SignalPoint { Time = 30.0, DifferenceValue = 10.0, CarrierValue = 440.0, VolumeValue = 100.0 };
        }

        private string name = "";

        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged("Name"); }
        }

        public SignalPoint[] points = null;
    }

    public class NoisePoint
    {
        public double Time { get; set; }
        public double SmoothnessValue { get; set; }
        public double VolumeValue { get; set; }
    }

    public class PresetModel : ModelBase
    {
        private string name = "";
        private string description = "";

        public PresetModel()
        {
            Name = "Enter your name";
            Description = "Enter your description";
            Signals = new ObservableCollection<Signal>
            {
                new Signal { Name = "Signal 1" }
            };
            noisePoints = new NoisePoint[3];
            noisePoints[0] = new NoisePoint { Time = 0.0, SmoothnessValue = 0.9, VolumeValue = 90.0 };
            noisePoints[1] = new NoisePoint { Time = 15.0, SmoothnessValue = 0.86, VolumeValue = 85.0 };
            noisePoints[2] = new NoisePoint { Time = 30.0, SmoothnessValue = 0.92, VolumeValue = 100.0 };
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
                    if (signal.points.First().Time < minTime)
                        minTime = signal.points.First().Time;

                    if (signal.points.First().Time > maxTime)
                        maxTime = signal.points.First().Time;

                    if (signal.points.Last().Time < minTime)
                        minTime = signal.points.Last().Time;

                    if (signal.points.Last().Time > maxTime)
                        maxTime = signal.points.Last().Time;
                }
                int secondsTime = (int)(maxTime - minTime);
                return new TimeSpan(secondsTime / 3600, secondsTime / 60 % 60, secondsTime % 60);
            }
        }

        public ObservableCollection<Signal> Signals { get; set; }

        public NoisePoint[] noisePoints = null;

        public string[] GraphsList
        {
            get
            {
                return Enum.GetNames(typeof(Graphs));
            }
        }
    }

    public enum Graphs
    {
        Carrier,
        Difference,
        Volume
    };
}
