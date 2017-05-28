using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using SharedLibrary;
using SharedLibrary.Code;
using SharedLibrary.Models;

using AudioCore.SampleProviders;

namespace BWGenerator.Models
{
    public class Signal : ModelBase
    {
        public Signal()
        {
            points = new List<SignalDataPoint>(2)
            {
                new SignalDataPoint { Time = 0.0, DifferenceValue = 8.0, CarrierValue = 330.0, VolumeValue = 50.0 },
                new SignalDataPoint { Time = 30.0, DifferenceValue = 10.0, CarrierValue = 440.0, VolumeValue = 75.0 }
            };
        }

        private string name = "";

        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }

        public List<SignalDataPoint> points = null;
    }

    public class PresetModel : ModelBase
    {
        private string name = "";
        private string description = "";
        private string statusMessage = string.Empty;


        public PresetModel()
        {
            Name = "Enter your name";
            Description = "Enter your description";
            Signals = new ObservableCollection<Signal>
            {
                new Signal { Name = "Signal 1" }
            };
            NoisePoints = new List<NoiseDataPoint>(3)
            {
                new NoiseDataPoint { Time = 0.0, SmoothnessValue = 0.9, VolumeValue = 90.0 },
                new NoiseDataPoint { Time = 15.0, SmoothnessValue = 0.86, VolumeValue = 85.0 },
                new NoiseDataPoint { Time = 30.0, SmoothnessValue = 0.92, VolumeValue = 100.0 }
            };
        }

        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; RaisePropertyChanged(); }
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
                return new TimeSpan(0, 0, secondsTime);
            }
        }

        public string StatusMessage
        {
            get
            {
                return statusMessage;
            }

            set
            {
                statusMessage = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Signal> Signals { get; set; }

        public List<NoiseDataPoint> NoisePoints { get; set; } = null;

        public string[] GraphsList
        {
            get
            {
                return Enum.GetNames(typeof(Graphs));
            }
        }

        public void ExportPresetAsWAV(string filename)
        {
            // saving as 16-bit file by default
            ModelledSampleProvider provider = new ModelledSampleProvider(Signals.Select(x => x.points).ToList(), NoisePoints);

            int samplingRate = provider.WaveFormat.SampleRate;
            ulong samplesCount = (ulong)TotalLength.Seconds * (ulong)samplingRate * 2UL;

            float[] providerContent = new float[samplesCount];
            provider.Read(providerContent, 0, (int)samplesCount);

            short[] content = providerContent.Select(x => (short)(x * short.MaxValue)).ToArray();
            WavFile.Save(filename, content);

            StatusMessage = System.IO.Path.GetFileName(filename) + " saved successfully";
        }
    }

    public enum Graphs
    {
        Carrier,
        Difference,
        Volume
    };
}
