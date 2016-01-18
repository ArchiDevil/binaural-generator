using System;
using System.Collections.Generic;
using ExperimenterUI.Models;

namespace ExperimenterUI
{
    public class SensorsData
    {
        public double temperatureValue = 0.0;
        public double skinResistanceValue = 0.0;
        public double motionValue = 0.0;
        public double pulseValue = 0.0;
    }

    [Serializable]
    internal class LoggerData
    {
        internal DateTime                            sessionStart;
        internal DateTime                            sessionEnd;
        internal bool                                sessionStarted = false;
        internal Dictionary<int, byte[]>             experimenterVoiceData = new Dictionary<int, byte[]>();
        internal Dictionary<int, byte[]>             subjectVoiceData = new Dictionary<int, byte[]>();
        internal Dictionary<int, SensorsData>        sensorsData = new Dictionary<int, SensorsData>();
        internal Dictionary<int, SignalViewModel[]>  signalsData = new Dictionary<int, SignalViewModel[]>();
        internal Dictionary<int, NoiseViewModel>     noiseData = new Dictionary<int, NoiseViewModel>();
    }
}
