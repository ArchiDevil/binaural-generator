using System;
using NAudio.Wave;

namespace AudioCore
{
    internal class Record : IDisposable
    {
        private WaveIn input = new WaveIn();
        private bool recordingEnabled = false;

        internal Record(int rate, int bits, int channels)
        {
            input.WaveFormat = new WaveFormat(rate, bits, channels);
            input.DataAvailable += Input_DataAvailable;
        }

        internal void StartRecording()
        {
            input.StartRecording();
            recordingEnabled = true;
        }

        internal void StopRecording()
        {
            input.StopRecording();
            recordingEnabled = false;
        }

        internal bool IsRecordingEnabled()
        {
            return recordingEnabled;
        }

        private void Input_DataAvailable(object sender, WaveInEventArgs e)
        {
            RecorderInput(sender, e);
        }

        public void Dispose()
        {
            ((IDisposable)input).Dispose();
        }

        internal delegate void RecordInputHandler(object sender, WaveInEventArgs e);
        internal event RecordInputHandler RecorderInput = delegate { };
    }
}
