namespace SharedContent.Models
{
    public class BasicNoiseModel : ModelBase
    {
        public bool enabled = false;
        public float gain = 100.0f;
        public int smoothness = 1;

        public BasicNoiseModel()
        {
        }

        public BasicNoiseModel(BasicNoiseModel basicNoiseModel)
        {
            enabled = basicNoiseModel.enabled;
            gain = basicNoiseModel.gain;
            smoothness = basicNoiseModel.smoothness;
        }
    }
}
