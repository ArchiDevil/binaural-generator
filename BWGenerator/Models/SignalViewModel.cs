using System;
using System.Collections.Generic;
using System.Linq;

using SharedLibrary.Code;
using SharedLibrary.Models;

namespace BWGenerator.Models
{
    public class SignalViewModel : ModelBase
    {
        public int StartTimeSeconds
        {
            get
            {
                return (int)currentSignal.points.FirstOrDefault().Time;
            }
            set
            {
                if (value < 0.0 || value > EndTimeSeconds)
                    return;

                currentSignal.points.FirstOrDefault().Time = value;
                for (int i = 1; i < currentSignal.points.Count; i++)
                {
                    if (value > currentSignal.points[i].Time)
                    {
                        currentSignal.points.RemoveAt(i);
                    }
                }

                noisePoints.FirstOrDefault().Time = value;
                for (int i = 1; i < noisePoints.Count; i++)
                {
                    if (value > noisePoints[i].Time)
                    {
                        noisePoints.RemoveAt(i);
                    }
                }

                RaisePropertyChanged();
                RaisePropertyChanged("Duration");
            }
        }

        public int EndTimeSeconds
        {
            get
            {
                return (int)currentSignal.points.LastOrDefault().Time;
            }
            set
            {
                if (value < 0.0 || value < StartTimeSeconds)
                    return;

                currentSignal.points.LastOrDefault().Time = value;
                for (int i = 0; i < currentSignal.points.Count - 1; i++)
                {
                    if (value < currentSignal.points[i].Time)
                    {
                        currentSignal.points.RemoveAt(i);
                    }
                }

                noisePoints.LastOrDefault().Time = value;
                for (int i = 0; i < noisePoints.Count - 1; i++)
                {
                    if (value < noisePoints[i].Time)
                    {
                        noisePoints.RemoveAt(i);
                    }
                }

                RaisePropertyChanged();
                RaisePropertyChanged("Duration");
            }
        }

        public string SignalName
        {
            get
            {
                return currentSignal.Name;
            }
            set
            {
                currentSignal.Name = value;
                RaisePropertyChanged();
            }
        }

        public TimeSpan Duration
        {
            get { return new TimeSpan(0, 0, EndTimeSeconds - StartTimeSeconds); }
        }

        private Signal currentSignal = null;
        private List<NoiseDataPoint> noisePoints = null;

        public SignalViewModel(Signal currentSignal, List<NoiseDataPoint> noisePoints)
        {
            this.currentSignal = currentSignal;
            this.noisePoints = noisePoints;
        }
    }
}
