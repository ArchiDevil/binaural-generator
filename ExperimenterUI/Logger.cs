using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ExperimenterUI.Models;

namespace ExperimenterUI
{
    public class Logger
    {
        LoggerData _data = new LoggerData();

        public void StartSession()
        {
            _data.sessionStart = DateTime.Now;
            _data.sessionStarted = true;
        }

        public void EndSession()
        {
            _data.sessionStarted = false;
            _data.sessionEnd = DateTime.Now;
        }

        public void LogSensors(SensorsData sensorsData)
        {
            // log sensors data
            if (!_data.sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - _data.sessionStart;
            _data.sensorsData.Add((int)delta.TotalMilliseconds, sensorsData);
        }

        public void LogSubjectVoice(byte[] buffer)
        {
            if (!_data.sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - _data.sessionStart;
            _data.subjectVoiceData.Add((int)delta.TotalMilliseconds, buffer);
        }

        public void LogExperimenterVoice(byte[] buffer)
        {
            if (!_data.sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - _data.sessionStart;
            _data.experimenterVoiceData.Add((int)delta.TotalMilliseconds, buffer);
        }

        public void LogSignalsChange(SignalViewModel[] signals, NoiseViewModel noise)
        {
            if (!_data.sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - _data.sessionStart;
            _data.signalsData.Add((int)delta.TotalMilliseconds, signals);
            _data.noiseData.Add((int)delta.TotalMilliseconds, noise);
        }

        public void DumpData(string filename)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(filename, FileMode.Create);
            formatter.Serialize(stream, _data);
            stream.Close();
        }
    }
}
