namespace SharedContent.Models
{
    public class BasicSignalModel : ModelBase
    {
        public bool enabled = false;
        public float gain = 100.0f;
        public float frequency = 440.0f;
        public float difference = 10.0f;

        public BasicSignalModel()
        {
        }

        public BasicSignalModel(BasicSignalModel basicSignalModel)
        {
            enabled = basicSignalModel.enabled;
            gain = basicSignalModel.gain;
            frequency = basicSignalModel.frequency;
            difference = basicSignalModel.difference;
        }
    }
}
