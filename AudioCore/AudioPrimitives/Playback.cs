using System;
using AudioCore.SampleProviders;
using NAudio.Wave;

namespace AudioCore.AudioPrimitives
{
    internal class Playback : IDisposable
    {
        IWavePlayer _driverOut = new WaveOutEvent();
        SampleProvider _sampleProvider = null;

        internal float Volume
        {
            get { return _sampleProvider.Gain; }
            set { _sampleProvider.Gain = value; }
        }

        internal Playback(SampleProvider sampleProvider)
        {
            _sampleProvider = sampleProvider;
            _driverOut.Init(sampleProvider);
        }

        public void Dispose()
        {
            _driverOut.Dispose();
        }

        internal bool Enabled
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
