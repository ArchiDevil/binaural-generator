using AudioCore.SampleProviders;

namespace AudioCore
{
    public class LocalAudioLayer
    {
        internal Record recorder = null;
        internal Playback playback = null;
        internal ConstantSampleProvider sampleProvider = null;

        public LocalAudioLayer()
        {
            recorder = new Record(44100, 16, 1);
            sampleProvider = new ConstantSampleProvider();
            playback = new Playback(sampleProvider);
        }

        public virtual void SetSignalSettings(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
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
