using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace AudioCore.SampleProviders
{
    internal class BufferedProvider : SampleProvider
    {
        private List<float> _buffer = null;
        private int _sampleRate = 0;

        internal BufferedProvider(int sampleRate) : base()
        {
            _sampleRate = sampleRate;
            _buffer = new List<float>();
            _buffer.AddRange(new float[sampleRate * 2]);
        }

        internal void AddSamples(float[] buffer, int sampleRate)
        {
            Contract.Requires(sampleRate == _sampleRate, "Sample rate must be equal to sample rate which was set on object creation");
            _buffer.AddRange(buffer);
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            if (count > _buffer.Count)
            {
                _buffer.CopyTo(0, buffer, offset, _buffer.Count);
                for (int i = 0; i < count - _buffer.Count; ++i)
                    buffer[i + offset + _buffer.Count] = 0.0f;
                _buffer.Clear();
            }
            else
            {
                _buffer.CopyTo(0, buffer, offset, count);
                _buffer.RemoveRange(0, count);
            }
            return count;
        }
    }
}
