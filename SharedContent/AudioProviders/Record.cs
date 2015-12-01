using NAudio.Wave;
using System;

namespace SharedLibrary.AudioProviders
{
    public class Record
    {
        WaveIn input = new WaveIn();
        WaveFormat format = new WaveFormat(8000, 16, 1);
        WaveFileWriter writer = null;

        public Record()
        {
            input.WaveFormat = format;
            input.DataAvailable += RecordInput;
            input.RecordingStopped += RecordingStopped;
        }

        public void StartRecording(string outputFileName)
        {
            writer = new WaveFileWriter(outputFileName, format);
            input.StartRecording();
        }

        public void StopRecording()
        {
            input.StopRecording();
        }

        private void RecordInput(object sender, WaveInEventArgs e)
        {
            if (writer != null)
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void RecordingStopped(object sender, StoppedEventArgs e)
        {
            writer.Close();
            writer = null;
        }
    }
}
