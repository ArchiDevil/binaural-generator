using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class Logger
    {
        private DateTime sessionStart;
        private DateTime sessionEnd;
        private bool sessionStarted = false;
        private Dictionary<int, byte[]> experimenterVoiceData = new Dictionary<int, byte[]>();
        private Dictionary<int, byte[]> subjectVoiceData = new Dictionary<int, byte[]>();
        private Dictionary<int, SensorsData> sensorsData = new Dictionary<int, SensorsData>();
        private Dictionary<int, SignalViewModel[]> signalsData = new Dictionary<int, SignalViewModel[]>();
        private Dictionary<int, NoiseViewModel> noiseData = new Dictionary<int, NoiseViewModel>();

        public void StartSession()
        {
            sessionStart = DateTime.Now;
            sessionStarted = true;
        }

        public void EndSession()
        {
            sessionStarted = false;
            sessionEnd = DateTime.Now;
        }

        public void LogSensors(SensorsData sensorsData)
        {
            // log sensors data
            if (!sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - sessionStart;
            this.sensorsData.Add(delta.Seconds, sensorsData);
        }

        public void LogSubjectVoice(byte[] buffer)
        {
            if (!sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - sessionStart;
            subjectVoiceData.Add(delta.Seconds, buffer);
        }

        public void LogExperimenterVoice(byte[] buffer)
        {
            if (!sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - sessionStart;
            experimenterVoiceData.Add(delta.Seconds, buffer);
        }

        public void LogSignalsChange(SignalViewModel[] signals, NoiseViewModel noise)
        {
            if (!sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - sessionStart;
            signalsData.Add(delta.Seconds, signals);
            noiseData.Add(delta.Seconds, noise);
        }

        public void DumpData(string filename)
        {
        }
    }
}
