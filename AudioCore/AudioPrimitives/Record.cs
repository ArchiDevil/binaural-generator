using System;
using NAudio.Wave;

namespace AudioCore.AudioPrimitives
{
    internal class RecoderInputEventArgs : EventArgs
    {
        internal RecoderInputEventArgs(byte[] bytes, int recorded, WaveFormat format)
        {
            Buffer = bytes;
            BytesRecorded = recorded;
            Format = format;
        }

        public byte[] Buffer { get; }

        public int BytesRecorded { get; }

        public WaveFormat Format { get; }
    }

    internal class Record : IDisposable
    {
        private WaveIn _input = new WaveIn();
        private bool _recordingEnabled = false;

        internal delegate void RecordInputHandler(object sender, RecoderInputEventArgs e);
        internal event RecordInputHandler RecorderInput;

        internal Record(int rate, int bits, int channels)
        {
            _input.WaveFormat = new WaveFormat(rate, bits, channels);
            _input.DataAvailable += Input_DataAvailable;
        }

        internal Record(int rate, int bits, int channels, int bufferLength)
        {
            _input.WaveFormat = new WaveFormat(rate, bits, channels);
            _input.BufferMilliseconds = bufferLength;
            _input.DataAvailable += Input_DataAvailable;
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

        internal int DevicesCount => WaveIn.DeviceCount;

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

        private void Input_DataAvailable(object sender, WaveInEventArgs e)
        {
            RecorderInput?.Invoke(sender, new RecoderInputEventArgs(e.Buffer, e.BytesRecorded, _input.WaveFormat));
        }

        public void Dispose()
        {
            _input.Dispose();
        }
    }
}
