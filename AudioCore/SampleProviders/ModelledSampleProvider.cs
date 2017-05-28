using System;
using System.Collections.Generic;
using System.Linq;

using SharedLibrary.Code;

namespace AudioCore.SampleProviders
{
    public static class HelperFunctions
    {
        public static double Lerp(double a, double b, double k)
        {
            return a * (1 - k) + b * k;
        }

        public static float[] GetSamplesValues(List<SignalDataPoint> dataPoints, double time, int samplingRate)
        {
            float[] samples = new float[2];

            // check if time is inside dataPoints
            if (time < dataPoints.First().Time)
                return samples;

            if (time > dataPoints.Last().Time)
                return samples;

            for (int i = 0; i < dataPoints.Count - 1; ++i)
            {
                if (time >= dataPoints[i].Time && time < dataPoints[i + 1].Time)
                {
                    SignalDataPoint point1 = dataPoints[i];
                    SignalDataPoint point2 = dataPoints[i + 1];

                    double k = (time - point1.Time) / (point2.Time - point1.Time);
                    double leftFrequency = Lerp(point1.CarrierValue, point2.CarrierValue, k) - Lerp(point1.DifferenceValue, point2.DifferenceValue, k);
                    double rightFrequency = Lerp(point1.CarrierValue, point2.CarrierValue, k);

                    samples[0] = (float)(Math.Sin(time * leftFrequency * 2 * Math.PI) * Lerp(point1.VolumeValue / 100.0, point2.VolumeValue / 100.0, k));
                    samples[1] = (float)(Math.Sin(time * rightFrequency * 2 * Math.PI) * Lerp(point1.VolumeValue / 100.0, point2.VolumeValue / 100.0, k));
                }
            }
            return samples;
        }
    }

    public class ModelledSampleProvider : SampleProvider
    {
        private List<List<SignalDataPoint>> _signalDataPoints = null;
        private List<NoiseDataPoint> _noiseDataPoints = null;

        private ulong _nSample = 0;
        private double _time = 0.0; // time in seconds

        public ModelledSampleProvider(List<List<SignalDataPoint>> signalDataPoints, List<NoiseDataPoint> noiseDataPoints)
        {
            _signalDataPoints = signalDataPoints;
            _noiseDataPoints = noiseDataPoints;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            Array.Clear(buffer, offset, count);
            for (int i = offset; i < offset + count; i += 2)
            {
                // add signal
                float leftSignalValue = 0.0f;
                float rightSignalValue = 0.0f;
                foreach (var signal in _signalDataPoints)
                {
                    float[] samples = HelperFunctions.GetSamplesValues(signal, _time, WaveFormat.SampleRate);
                    leftSignalValue += samples[0];
                    rightSignalValue += samples[1];
                }

                // add noise
                // TODO: create noise getter
                float leftNoiseValue = 0.0f;
                float rightNoiseValue = 0.0f;

                // write value
                buffer[i] = leftSignalValue + leftNoiseValue;
                buffer[i + 1] = rightSignalValue + rightNoiseValue;

                _time += 1.0 / WaveFormat.SampleRate;
                _nSample++;
            }
            return count;
        }
    }
}
