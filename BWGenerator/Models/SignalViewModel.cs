using System.Linq;
using System.Windows;

using SharedLibrary.Models;

namespace BWGenerator.Models
{
    public class SignalViewModel : ModelBase
    {
        private Signal currentSignal = null;

        public int startTimeSeconds
        {
            get
            {
                if (currentSignal.points.Count() > 0)
                    return (int)currentSignal.points.First().Time;
                else
                    return 0;
            }
            set
            {
                MessageBox.Show("LALALA");
            }
        }

        public int endTimeSeconds
        {
            get
            {
                if (currentSignal.points.Count() > 0)
                    return (int)currentSignal.points.Last().Time;
                else
                    return 0;
            }
            set
            {
                MessageBox.Show("LALALA");
            }
        }

        public string signalName
        {
            get { return currentSignal.Name; }
            set { currentSignal.Name = value; RaisePropertyChanged("signalName"); }
        }

        public SignalViewModel(Signal currentSignal)
        {
            this.currentSignal = currentSignal;
        }
    }
}
