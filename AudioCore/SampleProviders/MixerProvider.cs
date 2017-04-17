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
                throw new ArgumentNullException("provider");

            if (waveFormat != null)
            {
                if (!waveFormat.Equals(provider.WaveFormat))
                    throw new ArgumentException("Wave formats in the all providers must be the same for now");
            }
            else
            {
                waveFormat = provider.WaveFormat;
            }

            providers.Add(provider);
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            for(int i = offset; i < offset + count; ++i)
                buffer[i] = 0.0f;

            foreach (var provider in providers)
            {
                float[] temporaryBuffer = new float[count];

                int providerSamples = provider.Read(temporaryBuffer, offset, count);
                if (providerSamples != count)
                    throw new ApplicationException("Provider returned wrong number of samples");

                for (int i = 0; i < temporaryBuffer.Length; ++i)
                    buffer[i + offset] += temporaryBuffer[i];
            }

            return count;
        }
    }
}
