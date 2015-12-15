using System;
using NAudio.Wave;

namespace SharedLibrary.AudioProviders
{
    public class Playback
    {
        IWavePlayer driverOut = new WaveOutEvent();
        SampleProvider sampleProvider = null;
        BufferedWaveProvider bufferStream = null;

        public float Volume
        {
            get { return sampleProvider.Gain; }
            set { sampleProvider.Gain = value; }
        }

        public Playback(SampleProvider sampleProvider)
        {
            this.sampleProvider = sampleProvider;
            driverOut.Init(sampleProvider);
        }

        public Playback(int rate, int bits, int channels)
        {
            bufferStream = new BufferedWaveProvider(new WaveFormat(rate, bits, channels));
            driverOut.Init(bufferStream);
        }

        public void Play()
        {
            driverOut.Play();
        }

        public void Pause()
        {
            driverOut.Pause();
        }

        public void Stop()
        {
            driverOut.Stop();
        }

        public void AddSamples(byte[] data)
        {
            bufferStream.AddSamples(data, 0, data.Length);
        }
    }
}
