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
        private DateTime _sessionStart;
        private DateTime _sessionEnd;
        private bool _sessionStarted = false;
        private Dictionary<int, byte[]> _experimenterVoiceData = new Dictionary<int, byte[]>();
        private Dictionary<int, byte[]> _subjectVoiceData = new Dictionary<int, byte[]>();
        private Dictionary<int, SensorsData> _sensorsData = new Dictionary<int, SensorsData>();
        private Dictionary<int, SignalViewModel[]> _signalsData = new Dictionary<int, SignalViewModel[]>();
        private Dictionary<int, NoiseViewModel> _noiseData = new Dictionary<int, NoiseViewModel>();

        public void StartSession()
        {
            _sessionStart = DateTime.Now;
            _sessionStarted = true;
        }

        public void EndSession()
        {
            _sessionStarted = false;
            _sessionEnd = DateTime.Now;
        }

        public void LogSensors(SensorsData sensorsData)
        {
            // log sensors data
            if (!_sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - _sessionStart;
            this._sensorsData.Add(delta.Seconds, sensorsData);
        }

        public void LogSubjectVoice(byte[] buffer)
        {
            if (!_sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - _sessionStart;
            _subjectVoiceData.Add(delta.Seconds, buffer);
        }

        public void LogExperimenterVoice(byte[] buffer)
        {
            if (!_sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - _sessionStart;
            _experimenterVoiceData.Add(delta.Seconds, buffer);
        }

        public void LogSignalsChange(SignalViewModel[] signals, NoiseViewModel noise)
        {
            if (!_sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - _sessionStart;
            _signalsData.Add(delta.Seconds, signals);
            _noiseData.Add(delta.Seconds, noise);
        }

        public void DumpData(string filename)
        {
        }
    }
}
