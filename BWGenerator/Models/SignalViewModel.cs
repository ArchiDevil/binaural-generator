using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharedContent.Models;

namespace BWGenerator.Models
{
    public class SignalViewModel : ModelBase
    {
        private PresetModel.Signal currentSignal = null;

        public int startTimeSeconds
        {
            get
            {
                if (currentSignal.SignalPoints.Count > 0)
                    return (int)currentSignal.SignalPoints.First().Time;
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
                if (currentSignal.SignalPoints.Count > 0)
                    return (int)currentSignal.SignalPoints.Last().Time;
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

        public SignalViewModel(PresetModel.Signal currentSignal)
        {
            this.currentSignal = currentSignal;
        }
    }
}
