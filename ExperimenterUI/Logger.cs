using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimenterUI
{
    public class Logger
    {
        private DateTime sessionStart;
        private DateTime sessionEnd;
        private bool sessionStarted = false;
        private Dictionary<int, byte[]> experimenterVoiceData = new Dictionary<int, byte[]>();
        private Dictionary<int, byte[]> subjectVoiceData = new Dictionary<int, byte[]>();

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

        public void LogSensors()
        {
            // log sensors data
        }

        public void LogSubjectVoice()
        {
            // log subject voice
        }

        public void LogExperimenterVoice(byte[] buffer)
        {
            if (!sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - sessionStart;
            experimenterVoiceData.Add(delta.Seconds, buffer);
        }

        public void LogSignalsChange(byte[] buffer)
        {
            if (!sessionStarted)
                return;

            DateTime eventTime = DateTime.Now;
            TimeSpan delta = eventTime - sessionStart;
            subjectVoiceData.Add(delta.Seconds, buffer);
        }

        public void DumpData(string filename)
        {

        }
    }
}
