namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    #endregion //Using Directives

    /// <summary>
    /// Secure random generator
    ///
    /// <https://stackoverflow.com/questions/42426420/how-to-generate-a-cryptographically-secure-random-integer-within-a-range>
    ///
    /// </summary>
    public class RandomGenerator : IDisposable
    {
        #region Fields

        private readonly RNGCryptoServiceProvider randomNumberGenerator;

        #endregion //Fields

        #region Constructors

        public RandomGenerator()
        {
            randomNumberGenerator = new RNGCryptoServiceProvider();
        }

        #endregion //Constructors

        #region Methods

        public int Next(int minimumValue, int maximumExclusiveValue)
        {
            if (minimumValue == maximumExclusiveValue)
            {
                return minimumValue;
            }
            if (minimumValue > maximumExclusiveValue)
            {
                throw new ArgumentOutOfRangeException($"{nameof(minimumValue)} must be lower than {nameof(maximumExclusiveValue)}");
            }
            var diff = (long)maximumExclusiveValue - minimumValue;
            var upperBound = uint.MaxValue / diff * diff;
            uint uInteger;
            do
            {
                uInteger = GetRandomUInt();
            } while (uInteger >= upperBound);
            return (int)(minimumValue + (uInteger % diff));
        }

        private uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            var buffer = new byte[bytesNumber];
            randomNumberGenerator.GetBytes(buffer);
            return buffer;
        }

        private bool _disposed;

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed state (managed objects).
                randomNumberGenerator?.Dispose();
            }

            _disposed = true;
        }

        #endregion //Methods
    }
}
