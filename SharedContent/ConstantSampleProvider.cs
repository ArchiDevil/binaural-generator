using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedContent.Models;

namespace SharedContent.AudioProviders
{
    public class ConstantSampleProvider : SampleProvider
    {
        private BasicSignalModel[] signals = null;

        public ConstantSampleProvider(BasicSignalModel[] signals) : base()
        {
            this.signals = signals;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            int outIndex = offset;

            for (int i = 0; i < count / waveFormat.Channels; i++)
            {
                double leftFrequency = 0.0;
                double rightFrequency = 0.0;

                double leftSampleValue = 0.0;
                double rightSampleValue = 0.0;

                double multiple = SharedFuncs.TwoPi / waveFormat.SampleRate;

                foreach (var signal in signals)
                {
                    if (!signal.enabled)
                        continue;

                    leftFrequency = signal.frequency;
                    rightFrequency = leftFrequency - signal.difference;

                    leftSampleValue += Gain * signal.gain / 100.0f * Math.Sin(nSample * multiple * leftFrequency);
                    rightSampleValue += Gain * signal.gain / 100.0f * Math.Sin(nSample * multiple * rightFrequency);
                }

                // left value
                buffer[outIndex++] = (float)leftSampleValue;
                // right value
                buffer[outIndex++] = (float)rightSampleValue;

                nSample++;
                time += 1.0 / waveFormat.SampleRate;
            }

            return count;
        }
    }
}
