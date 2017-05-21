using System;
using System.Linq;
using SharedLibrary.Models;

namespace BWGenerator.Models
{
    public class SignalViewModel : ModelBase
    {
        public int StartTimeSeconds
        {
            get
            {
                return (currentSignal.points.Count > 0) ? (int)currentSignal.points.First().Time : 0;
            }
            set
            {
                if (value < 0.0 || value > EndTimeSeconds)
                    return;

                currentSignal.points[0].Time = value;
                for (int i = 1; i < currentSignal.points.Count; i++)
                {
                    if (value > currentSignal.points[i].Time)
                    {
                        currentSignal.points.RemoveAt(i);
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
                return (currentSignal.points.Count > 0) ? (int)currentSignal.points.Last().Time : 0;
            }
            set
            {
                if (value < 0.0 || value < StartTimeSeconds)
                    return;

                currentSignal.points[currentSignal.points.Count - 1].Time = value;
                for (int i = 0; i < currentSignal.points.Count - 1; i++)
                {
                    if (value < currentSignal.points[i].Time)
                    {
                        currentSignal.points.RemoveAt(i);
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

        public SignalViewModel(Signal currentSignal)
        {
            this.currentSignal = currentSignal;
        }
    }
}
