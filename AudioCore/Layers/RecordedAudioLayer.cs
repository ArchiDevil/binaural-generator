using AudioCore.AudioPrimitives;

namespace AudioCore.Layers
{
    public class RecordedAudioLayer : LocalAudioLayer
    {
        internal Record _recorder = null;

        public RecordedAudioLayer()
            : base()
        {
            _recorder = new Record(44100, 16, 1);
        }

        public bool RecordingEnabled
        {
            get { return _recorder.Enabled; }
            set { _recorder.Enabled = value; }
        }

        public int AudioInDevicesCount
        {
            get { return _recorder.DevicesCount; }
        }
    }
}
