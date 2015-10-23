using NAudio.Wave;

namespace SharedLibrary.AudioProviders
{
    public class Playback
    {
        IWavePlayer driverOut = new WaveOutEvent();
        SampleProvider sampleProvider = null;

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
    }
}
