using System.Collections.Generic;
using System.Collections.ObjectModel;

using SharedContent.Models;

namespace BWGenerator.Models
{
    public class PresetModel : ModelBase
    {
        public class Signal : ModelBase
        {
            public struct Point
            {
                public double time { get; set; }
                public double value { get; set; }
            }

            private string _name = "";

            public string Name
            {
                get { return _name; }
                set { _name = value; RaisePropertyChanged("Name"); }
            }

            public List<Point> DifferencePoints = new List<Point>();
            public List<Point> CarrierPoints = new List<Point>();
        }

        private string _name = "";
        private string _description = "";

        public PresetModel()
        {
            Name = "";
            Description = "";
            Signals = new ObservableCollection<Signal>();
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("Name"); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; RaisePropertyChanged("Description"); }
        }

        public ObservableCollection<Signal> Signals { get; set; }
    }
}
