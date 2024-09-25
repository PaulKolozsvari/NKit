namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    #endregion //Using Directives

    //https://stackoverflow.com/questions/42426420/how-can-i-generate-a-cryptographically-secure-random-integer-within-a-range
    public class SecureRandomNumberGenerator : IDisposable
    {
        #region Constructors

        public SecureRandomNumberGenerator()
        {
            _randomNumberGenerator = RandomNumberGenerator.Create();
        }

        #endregion //Constructors

        #region Methods

        private RandomNumberGenerator _randomNumberGenerator = null;

        public int GenerateRandomNumberInRange(int minimumValue, int maximumValue)
        {
            if (minimumValue >= maximumValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumValue), $"{minimumValue} must be less than {nameof(maximumValue)}");
            }
            int range = maximumValue - minimumValue + 1;
            byte[] uint32Buffer = new byte[4]; //Four bytes
            int result;
            do
            {
                _randomNumberGenerator.GetBytes(uint32Buffer);
                uint randomUint = BitConverter.ToUInt32(uint32Buffer, 0);
                result = (int)(randomUint % range);
            } while (result < 0 || result >= range);
            return minimumValue + result;
        }

        public void Dispose()
        {
            _randomNumberGenerator.Dispose();
        }

        #endregion //Methods
    }
}