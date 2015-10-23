using System;
using System.Linq;

using SharedLibrary.Models;

namespace BWGenerator.Models
{
    public class SignalViewModel : ModelBase
    {
        public int startTimeSeconds
        {
            get
            {
                return (currentSignal.points.Count() > 0) ? (int)currentSignal.points.First().Time : 0;
            }
            set
            {
                if (value < 0.0 || value > endTimeSeconds)
                    return;

                currentSignal.points[0].Time = value;
                for (int i = 1; i < currentSignal.points.Count(); i++)
                {
                    if (value > currentSignal.points[i].Time)
                    {
                        SharedLibrary.SharedFuncs.RemoveFromArrayByIndex(ref currentSignal.points, i);
                    }
                }

                RaisePropertyChanged("startTimeSeconds");
            }
        }

        public int endTimeSeconds
        {
            get
            {
                return (currentSignal.points.Count() > 0) ? (int)currentSignal.points.Last().Time : 0;
            }
            set
            {
                if (value < 0.0 || value < startTimeSeconds)
                    return;

                currentSignal.points[currentSignal.points.Count() - 1].Time = value;
                for (int i = 0; i < currentSignal.points.Count() - 1; i++)
                {
                    if (value < currentSignal.points[i].Time)
                    {
                        SharedLibrary.SharedFuncs.RemoveFromArrayByIndex(ref currentSignal.points, i);
                    }
                }

                RaisePropertyChanged("endTimeSeconds");
            }
        }

        public string signalName
        {
            get
            {
                return currentSignal.Name;
            }
            set
            {
                currentSignal.Name = value;
                RaisePropertyChanged("signalName");
            }
        }

        private Signal currentSignal = null;

        public SignalViewModel(Signal currentSignal)
        {
            this.currentSignal = currentSignal;
        }
    }
}
