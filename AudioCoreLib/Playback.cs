using System;

using NAudio.Wave;

using AudioCore.SampleProviders;

namespace AudioCore
{
    internal class Playback
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

        internal void Play()
        {
            driverOut.Play();
        }

        internal void Pause()
        {
            driverOut.Pause();
        }

        internal void Stop()
        {
            driverOut.Stop();
        }

        internal void AddSamples(byte[] data)
        {
            bufferStream.AddSamples(data, 0, data.Length);
        }
    }
}
