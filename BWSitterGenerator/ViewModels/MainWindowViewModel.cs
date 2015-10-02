using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedContent.Models;

namespace BWSitterGenerator.ViewModels
{
    class MainWindowViewModel : ModelBase
    {
        private bool[] signalsEnabled;

        public bool Signal1Enabled
        {
            get { return signalsEnabled[0]; }
            set {signalsEnabled[0] = value; RaisePropertyChanged("Signal1Enabled");}
        }

        public bool Signal2Enabled
        {
            get { return signalsEnabled[1]; }
            set {signalsEnabled[1] = value; RaisePropertyChanged("Signal2Enabled");}
        }

        public bool Signal3Enabled
        {
            get { return signalsEnabled[2]; }
            set {signalsEnabled[2] = value; RaisePropertyChanged("Signal3Enabled");}
        }

        public bool Signal4Enabled
        {
            get { return signalsEnabled[3]; }
            set {signalsEnabled[3] = value; RaisePropertyChanged("Signal4Enabled");}
        }

        public MainWindowViewModel()
        {
            signalsEnabled = new bool[4];
            signalsEnabled[0] = true;
        }
    }
}
