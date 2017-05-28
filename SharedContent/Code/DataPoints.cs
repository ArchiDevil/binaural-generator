using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Code
{
    public class BaseDataPoint
    {
        public double Time { get; set; } = 0.0;
    }

    public class SignalDataPoint : BaseDataPoint
    {
        public double DifferenceValue { get; set; } = 8.0;
        public double CarrierValue { get; set; } = 440.0;
        public double VolumeValue { get; set; } = 50.0;
    }
    public class NoiseDataPoint : BaseDataPoint
    {
        public double SmoothnessValue { get; set; } = 0.9;
        public double VolumeValue { get; set; } = 75.0;
    }
}
