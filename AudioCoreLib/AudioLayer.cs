using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioCore.SampleProviders;

namespace AudioCore
{
    public class AudioLayer
    {
        private Record recorder = null;
        private Playback playback = null;
        private ConstantSampleProvider sampleProvider = null;

        public AudioLayer()
        {
            recorder = new Record(44100, 16, 1);
            sampleProvider = new ConstantSampleProvider();
            playback = new Playback(sampleProvider);
        }

        public void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            sampleProvider.SetSignals(channelSignals, noiseSignal);
        }

        public void Start()
        {
            playback.Play();
        }

        public void Stop()
        {
            playback.Stop();
        }

        public bool RecordingEnabled
        {
            get
            {
                return recorder.IsRecordingEnabled();
            }

            set
            {
                if (value)
                    recorder.StartRecording();
                else
                    recorder.StopRecording();
            }
        }
    }
}
