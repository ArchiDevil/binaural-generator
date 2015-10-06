using System;
using System.Linq;

namespace SharedContent
{
    /// <summary>
    /// A class that provides a source of pink noise with a power spectrum
    /// density(PSD) proportional to 1/f^alpha.  "Regular" pink noise has a
    /// PSD proportional to 1/f, i.e.alpha=1.  However, many natural
    /// systems may require a different PSD proportionality.The value of
    /// alpha may be from 0 to 2, inclusive.The special case alpha=0
    /// results in white noise(directly generated random numbers) and
    /// alpha = 2 results in brown noise(integrated white noise).
    /// <p>
    /// The values are computed by applying an IIR filter to generated
    /// Gaussian random numbers.The number of poles used in the filter
    /// may be specified.For each number of poles there is a limiting
    /// frequency below which the PSD becomes constant.Values as low as
    /// 1-3 poles produce relatively good results, however these values
    /// will be concentrated near zero.  Using a larger number of poles
    /// will allow more low frequency components to be included, leading to
    /// more variation from zero.  However, the sequence is stationary,
    /// that is, it will always return to zero even with a large number of
    /// poles.
    /// </summary>
    public class NoiseGenerator
    {
        private int poles = 0;
        private double[] multipliers = null;
        private double[] values = null;
        private Random rnd = null;

        /// <summary>
        /// Generate pink noise from a specific randomness source
        /// specifying alpha and the number of poles.The larger the
        /// number of poles, the lower are the lowest frequency components
        /// that are amplified.
        /// </summary>
        /// <param name="alpha">the exponent of the pink noise, 1/f^alpha.</param>
        /// <param name="poles">the number of poles to use.</param>
        /// <param name="random">the randomness source.</param>
        public NoiseGenerator(double alpha, int poles)
        {
            if (alpha < 0.0 || alpha > 2.0)
                throw new ArgumentException("Invalid pink noise alpha = " + alpha);

            rnd = new Random();
            this.poles = poles;
            multipliers = new double[poles];
            values = new double[poles];

            double a = 1.0;
            for (int i = 0; i < poles; i++)
            {
                a = (i - alpha / 2.0) * a / (i + 1.0);
                multipliers[i] = a;
            }

            // Fill the history with random values
            for (int i = 0; i < 5 * poles; i++)
                NextValue();
        }

        /// <summary>
        /// Returns the next noise sample
        /// </summary>
        /// <returns>next sample</returns>
        public double NextValue()
        {
            double x = rnd.NextDouble() * 2.0 - 1.0;

            for (int i = 0; i < poles; i++)
                x -= multipliers[i] * values[i];

            double[] temp = new double[values.Count()];
            values.CopyTo(temp, 0);

            for (int i = 1; i < values.Count(); ++i)
                values[i] = temp[i - 1];

            values[0] = x;
            return x;
        }
    }
}
