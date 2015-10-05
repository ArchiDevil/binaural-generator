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
        private BasicSignalModel[] channelSignals = null;
        private BasicNoiseModel noiseSignal = null;

        public ConstantSampleProvider(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal) : base()
        {
            this.channelSignals = channelSignals;
            this.noiseSignal = noiseSignal;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            ReadSignals(buffer, offset, count);
            ReadNoise(buffer, offset, count);
            return count;
        }

        private int ReadSignals(float[] buffer, int offset, int count)
        {
            int outIndex = offset;
            int returnVal = 0;

            for (int i = 0; i < count / waveFormat.Channels; i++)
            {
                double leftFrequency = 0.0;
                double rightFrequency = 0.0;

                double leftSampleValue = 0.0;
                double rightSampleValue = 0.0;

                double multiple = SharedFuncs.TwoPi / waveFormat.SampleRate;

                foreach (var signal in channelSignals)
                {
                    if (!signal.enabled)
                        continue;

                    // return non-zero only if we wrote some values
                    returnVal = count;

                    leftFrequency = signal.frequency;
                    rightFrequency = leftFrequency - signal.difference;

                    leftSampleValue += Gain * signal.gain / 100.0f * Math.Sin(nSample * multiple * leftFrequency);
                    rightSampleValue += Gain * signal.gain / 100.0f * Math.Sin(nSample * multiple * rightFrequency);
                }

                // add signal here, cause noise can be applied before signal
                // left value
                buffer[outIndex++] += (float)leftSampleValue;
                // right value
                buffer[outIndex++] += (float)rightSampleValue;

                nSample++;
                time += 1.0 / waveFormat.SampleRate;
            }

            return returnVal;
        }

        private int ReadNoise(float[] buffer, int offset, int count)
        {
            // may be we need to save old array to avoid TICKS in sound
            int outIndex = offset;

            if (!noiseSignal.enabled)
                return count;

            // for now - mono
            double[] prevSampleValues = new double[noiseSignal.smoothness];
            Random randomizer = new Random();

            for (int i = 0; i < count / waveFormat.Channels; i++)
            {
                // save both of them, cause may be we need, 
                // sometimes and somewhere to use 
                // different values for different channels
                double leftSampleValue = 0.0;
                double rightSampleValue = 0.0;

                for (int j = 1; j < prevSampleValues.Count(); ++j)
                {
                    prevSampleValues[j - 1] = prevSampleValues[j];
                }
                prevSampleValues[prevSampleValues.Count() - 1] = randomizer.NextDouble() * 2.0 - 1.0;

                double noiseValue = 0.0;
                noiseValue = prevSampleValues.Sum();
                noiseValue /= prevSampleValues.Count();
                noiseValue *= Gain * noiseSignal.gain / 100.0f;

                // mono for now =(
                rightSampleValue = leftSampleValue = noiseValue;

                // add signal here, cause noise can be applied before signal
                // left value
                buffer[outIndex++] += (float)leftSampleValue;
                // right value
                buffer[outIndex++] += (float)rightSampleValue;
            }

            return count;
        }
    }
}
