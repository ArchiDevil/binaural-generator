using System;
using NAudio.Wave;

namespace AudioCore
{
    internal class Record : IDisposable
    {
        private WaveIn _input = new WaveIn();
        private bool _recordingEnabled = false;

        internal Record(int rate, int bits, int channels)
        {
            _input.WaveFormat = new WaveFormat(rate, bits, channels);
            _input.DataAvailable += Input_DataAvailable;
        }

        private void StartRecording()
        {
            _input.StartRecording();
            _recordingEnabled = true;
        }

        private void StopRecording()
        {
            _input.StopRecording();
            _recordingEnabled = false;
        }

        internal bool Enabled
        {
            get { return _recordingEnabled; }
            set
            {
                if (value)
                    StartRecording();
                else
                    StopRecording();
            }
        }

        private void Input_DataAvailable(object sender, WaveInEventArgs e)
        {
            RecorderInput(sender, e);
        }

        public void Dispose()
        {
            ((IDisposable)_input).Dispose();
        }

        internal delegate void RecordInputHandler(object sender, WaveInEventArgs e);
        internal event RecordInputHandler RecorderInput = delegate { };
    }
}
