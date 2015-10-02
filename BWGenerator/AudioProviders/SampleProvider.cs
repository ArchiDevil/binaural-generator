using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

using BWGenerator.Models;

namespace BWGenerator.AudioProviders
{
    class SampleProvider : ISampleProvider
    {
        readonly WaveFormat waveFormat;
        readonly PresetModel currentPreset;

        private int nSample = 0;
        private double time = 0.0; // time in seconds
        private const double TwoPi = 2 * Math.PI;

        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        public float Gain { get; set; } = 1.0f;

        public static double Lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public SampleProvider(PresetModel currentPreset)
        {
            this.currentPreset = currentPreset;
            this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate: 44100, channels: 2);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int outIndex = offset;

            for (int i = 0; i < count / waveFormat.Channels; i++)
            {
                double leftFrequency = 0.0;
                double rightFrequency = 0.0;

                double leftSampleValue = 0.0;
                double rightSampleValue = 0.0;

                foreach (var signal in currentPreset.Signals)
                {
                    PresetModel.Signal.Point lastPoint = signal.CarrierPoints.Last();
                    if (time >= lastPoint.time)
                        time = 0.0;

                    int prevPointIndex = 0;
                    int nextPointIndex = 0;

                    for (int j = 0; j < signal.CarrierPoints.Count; ++j)
                    {
                        if (signal.CarrierPoints[j].time > time)
                        {
                            nextPointIndex = j;
                            prevPointIndex = j - 1;
                            break;
                        }
                    }

                    double koef = (time - signal.CarrierPoints[prevPointIndex].time)
                                         /
                                         (signal.CarrierPoints[nextPointIndex].time - signal.CarrierPoints[prevPointIndex].time);

                    leftFrequency = Lerp(signal.CarrierPoints[prevPointIndex].value, signal.CarrierPoints[nextPointIndex].value, koef);
                    rightFrequency = leftFrequency - Lerp(signal.DifferencePoints[prevPointIndex].value, signal.DifferencePoints[nextPointIndex].value, koef);
                }

                double multiple = TwoPi * leftFrequency / waveFormat.SampleRate;
                leftSampleValue = Gain * Math.Sin(nSample * multiple);
                // left value
                buffer[outIndex++] = (float)leftSampleValue;

                multiple = TwoPi * rightFrequency / waveFormat.SampleRate;
                rightSampleValue = Gain * Math.Sin(nSample * multiple);
                // right value
                buffer[outIndex++] = (float)rightSampleValue;

                nSample++;
                time += 1.0 / waveFormat.SampleRate;
            }

            return count;
        }
    }
}
