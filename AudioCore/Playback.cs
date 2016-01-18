using System;
using NAudio.Wave;
using AudioCore.SampleProviders;

namespace AudioCore
{
    internal class Playback : IDisposable
    {
        IWavePlayer _driverOut = new WaveOutEvent();
        SampleProvider _sampleProvider = null;
        BufferedWaveProvider _bufferStream = null;

        internal float Volume
        {
            get { return _sampleProvider.Gain; }
            set { _sampleProvider.Gain = value; }
        }

        internal Playback(SampleProvider sampleProvider)
        {
            this._sampleProvider = sampleProvider;
            _driverOut.Init(sampleProvider);
        }

        internal void AddSamples(byte[] data)
        {
            _bufferStream.AddSamples(data, 0, data.Length);
        }

        public void Dispose()
        {
            _driverOut.Dispose();
        }

        public bool Enabled
        {
            get { return _driverOut.PlaybackState == PlaybackState.Playing; }
            set
            {
                if (value)
                    _driverOut.Play();
                else
                    _driverOut.Stop();
            }
        }
    }
}
