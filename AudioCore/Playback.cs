using System;
using NAudio.Wave;
using AudioCore.SampleProviders;

namespace AudioCore
{
    internal class Playback : IDisposable
    {
        IWavePlayer driverOut = new WaveOutEvent();
        SampleProvider sampleProvider = null;
        BufferedWaveProvider bufferStream = null;

        internal float Volume
        {
            get { return sampleProvider.Gain; }
            set { sampleProvider.Gain = value; }
        }

        internal Playback(SampleProvider sampleProvider)
        {
            this.sampleProvider = sampleProvider;
            driverOut.Init(sampleProvider);
        }

        internal void AddSamples(byte[] data)
        {
            bufferStream.AddSamples(data, 0, data.Length);
        }

        public void Dispose()
        {
            driverOut.Dispose();
        }

        public bool Enabled
        {
            get { return driverOut.PlaybackState == PlaybackState.Playing; }
            set
            {
                if (value)
                    driverOut.Play();
                else
                    driverOut.Stop();
            }
        }
    }
}
