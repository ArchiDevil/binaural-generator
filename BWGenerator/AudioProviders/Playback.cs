using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

using BWGenerator.Models;

namespace BWGenerator.AudioProviders
{
    class Playback
    {
        IWavePlayer driverOut = new WaveOutEvent();
        SampleProvider sampleProvider = null;

        public float Volume
        {
            get { return sampleProvider.Gain; }
            set { sampleProvider.Gain = value; }
        }

        public Playback(PresetModel currentPreset)
        {
            sampleProvider = new SampleProvider(currentPreset);
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
