using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace AudioCore.SampleProviders
{
    internal class MixerProvider : SampleProvider
    {
        private WaveFormat waveFormat = null;
        List<SampleProvider> providers = null;

        internal MixerProvider() : base()
        {
            providers = new List<SampleProvider>();
        }

        internal void AddProvider(SampleProvider provider)
        {
            if (provider == null)
                throw new ArgumentException("Param mustn't be null", "provider");

            if (waveFormat != null)
            {
                if (waveFormat != provider.WaveFormat)
                    throw new ArgumentException("Wave formats in the all prodivers must be the same for now");
            }
            else
            {
                waveFormat = provider.WaveFormat;
            }

            providers.Add(provider);
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            float[] resultingBuffer = new float[count];
            foreach (var provider in providers)
            {
                float[] temporaryBuffer = new float[count];

                int providerSamples = provider.Read(temporaryBuffer, offset, count);
                if (providerSamples != count)
                    throw new ApplicationException("Provider returned wrong number of samples");

                for (int i = 0; i < temporaryBuffer.Length; ++i)
                    resultingBuffer[i] += temporaryBuffer[i];
            }

            return count;
        }
    }
}
