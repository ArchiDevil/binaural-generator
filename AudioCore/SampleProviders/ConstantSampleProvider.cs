﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCore.SampleProviders
{
    public class ConstantSampleProvider : SampleProvider
    {
        private BasicSignalModel[] _channelSignals = null;
        private BasicNoiseModel _noiseSignal = null;

        private double _lastSmoothness; // it's in indeterminate state to create generator on first use

        private double _previousLeftValue = 0.0;
        private double _previousRightValue = 0.0;

        public ConstantSampleProvider() : base()
        {
            _channelSignals = new BasicSignalModel[4];
            for (int i = 0; i < _channelSignals.Length; ++i)
                _channelSignals[i] = new BasicSignalModel();

            _noiseSignal = new BasicNoiseModel();
        }

        public ConstantSampleProvider(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal) : base()
        {
            this._channelSignals = channelSignals;
            this._noiseSignal = noiseSignal;
        }

        public void SetSignals(BasicSignalModel[] channelSignals, BasicNoiseModel noiseSignal)
        {
            this._channelSignals = channelSignals;
            this._noiseSignal = noiseSignal;
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; ++i)
                buffer[i] = 0.0f;

            int signalSamplesCount = ReadSignals(buffer, offset, count);
            int noiseSamplesCount = ReadNoise(buffer, offset, count);
            return Math.Max(signalSamplesCount, noiseSamplesCount);
        }

        private int ReadSignals(float[] buffer, int offset, int count)
        {
            int outIndex = offset;
            int returnVal = 0;

            for (int i = 0; i < count / _waveFormat.Channels; i++)
            {
                double leftFrequency = 0.0;
                double rightFrequency = 0.0;

                double leftSampleValue = 0.0;
                double rightSampleValue = 0.0;

                double multiple = UtilFuncs.TwoPi / _waveFormat.SampleRate;

                foreach (var signal in _channelSignals)
                {
                    if (!signal.enabled)
                        continue;

                    // return non-zero only if we wrote some values
                    returnVal = count;

                    leftFrequency = signal.frequency;
                    rightFrequency = leftFrequency - signal.difference;

                    leftSampleValue += Gain * signal.gain / 100.0f * Math.Sin(_nSample * multiple * leftFrequency);
                    rightSampleValue += Gain * signal.gain / 100.0f * Math.Sin(_nSample * multiple * rightFrequency);
                }

                // add signal here, cause noise can be applied before signal
                // left value
                buffer[outIndex++] += (float)leftSampleValue;
                // right value
                buffer[outIndex++] += (float)rightSampleValue;

                _nSample++;
                _time += 1.0 / _waveFormat.SampleRate;
            }

            return returnVal;
        }

        private int ReadNoise(float[] buffer, int offset, int count)
        {
            // may be we need to save old array to avoid TICKS in sound
            int outIndex = offset;

            if (!_noiseSignal.enabled)
                return count;

            if (_noiseSignal.smoothness != _lastSmoothness)
                _lastSmoothness = _noiseSignal.smoothness;

            Random rand = new Random();
            double noiseLeftValue = 0.0;
            double noiseRightValue = 0.0;
            // save both of them, cause may be we need, 
            // sometimes and somewhere to use 
            // different values for different channels
            double leftSampleValue = 0.0;
            double rightSampleValue = 0.0;

            for (int i = 0; i < count / _waveFormat.Channels; i++)
            {
                noiseLeftValue = rand.NextDouble() * 2.0 - 1.0;
                noiseLeftValue = _previousLeftValue * (_noiseSignal.smoothness) + noiseLeftValue * (1.0 - _noiseSignal.smoothness);
                noiseLeftValue *= Gain * _noiseSignal.gain / 100.0f;
                _previousLeftValue = noiseLeftValue;

                noiseRightValue = rand.NextDouble() * 2.0 - 1.0;
                noiseRightValue = _previousRightValue * (_noiseSignal.smoothness) + noiseRightValue * (1.0 - _noiseSignal.smoothness);
                noiseRightValue *= Gain * _noiseSignal.gain / 100.0f;
                _previousRightValue = noiseRightValue;

                // mono for now =(
                rightSampleValue = noiseRightValue;
                leftSampleValue = noiseLeftValue;

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
