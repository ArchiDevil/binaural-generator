using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace SharedLibrary.AudioProviders
{
    public class ModelledSampleProvider : SampleProvider
    {
        //readonly PresetModel currentPreset = null;

        //public ModelledSampleProvider(PresetModel currentPreset) : base()
        //{
        //    this.currentPreset = currentPreset;
        //}

        public override int Read(float[] buffer, int offset, int count)
        {
            //int outIndex = offset;

            //for (int i = 0; i < count / waveFormat.Channels; i++)
            //{
            //    double leftFrequency = 0.0;
            //    double rightFrequency = 0.0;

            //    double leftSampleValue = 0.0;
            //    double rightSampleValue = 0.0;

            //    double multiple = SharedFuncs.TwoPi / waveFormat.SampleRate;

            //    foreach (var signal in currentPreset.Signals)
            //    {
            //        PresetModel.Signal.Point lastPoint = signal.CarrierPoints.Last();
            //        if (time >= lastPoint.time)
            //            time = 0.0;

            //        int prevPointIndex = 0;
            //        int nextPointIndex = 0;

            //        for (int j = 0; j < signal.CarrierPoints.Count; ++j)
            //        {
            //            if (signal.CarrierPoints[j].time > time)
            //            {
            //                nextPointIndex = j;
            //                prevPointIndex = j - 1;
            //                break;
            //            }
            //        }

            //        double koef = (time - signal.CarrierPoints[prevPointIndex].time)
            //                             /
            //                             (signal.CarrierPoints[nextPointIndex].time - signal.CarrierPoints[prevPointIndex].time);

            //        leftFrequency = SharedFuncs.Lerp(signal.CarrierPoints[prevPointIndex].value, signal.CarrierPoints[nextPointIndex].value, koef);
            //        rightFrequency = leftFrequency - SharedFuncs.Lerp(signal.DifferencePoints[prevPointIndex].value, signal.DifferencePoints[nextPointIndex].value, koef);

            //        leftSampleValue += Gain * Math.Sin(nSample * multiple * leftFrequency);
            //        rightSampleValue += Gain * Math.Sin(nSample * multiple * rightFrequency);
            //    }

            //    // left value
            //    buffer[outIndex++] = (float)leftSampleValue;
            //    // right value
            //    buffer[outIndex++] = (float)rightSampleValue;

            //    nSample++;
            //    time += 1.0 / waveFormat.SampleRate;
            //}

            return count;
        }
    }
}
