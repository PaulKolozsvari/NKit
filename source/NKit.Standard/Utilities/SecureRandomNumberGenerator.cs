namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Cryptography;
    using System.Text;

    #endregion //Using Directives

    //https://stackoverflow.com/questions/42426420/how-can-i-generate-a-cryptographically-secure-random-integer-within-a-range
    public class SecureRandomNumberGenerator : IDisposable
    {
        #region Constructors

        public SecureRandomNumberGenerator(int minimumValue, int maximumValue, List<int> excludedNumbers)
        {
            _randomNumberGenerator = RandomNumberGenerator.Create();
            _excludedNumbers = new Dictionary<int, object>(); //We convert the list to a dictionary because it's faster to lookupn against a dictionary than a list.
            if (excludedNumbers != null)
            {
                foreach (int i in excludedNumbers)
                {
                    if (!_excludedNumbers.ContainsKey(i))
                    {
                        _excludedNumbers.Add(i, null);
                    }
                }
            }
            if (minimumValue >= maximumValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumValue), $"{minimumValue} must be less than {nameof(maximumValue)}");
            }
            _minimumValue = minimumValue;
            _maximumValue = maximumValue;
        }

        #endregion //Constructors

        #region Methods

        private RandomNumberGenerator _randomNumberGenerator = null;
        private Dictionary<int, object> _excludedNumbers;
        private int _minimumValue;
        private int _maximumValue;

        public bool AddExcludedNumber(int number)
        {
            if (_excludedNumbers.ContainsKey(number))
            {
                return false;
            }
            _excludedNumbers.Add(number, null);
            return true;
        }

        public bool RemoveExcludedNumber(int number)
        {
            if (!_excludedNumbers.ContainsKey(number))
            {
                return false;
            }
            _excludedNumbers.Remove(number);
            return true;
        }

        /// <summary>
        /// Generates a secure random number in a certain range that does not exist in the list of execluded numbers.
        /// </summary>
        /// <param name="minimumValue"></param>
        /// <param name="maximumValue"></param>
        /// <param name="excludedNumbers">The numbers to exclude. This is a </param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int GenerateRandomNumberInRange(bool addResultToExcludedNumbers)
        {
            int range = _maximumValue - _minimumValue + 1;
            byte[] uint32Buffer = new byte[4]; //Four bytes
            int result = -1;
            while (true)
            {
                _randomNumberGenerator.GetBytes(uint32Buffer);
                uint randomUint = GetRandomUInt();
                result += _minimumValue;
                result = (int)(randomUint % range);
                if (result > 0 && result < range && !_excludedNumbers.ContainsKey(result))
                {
                    if (addResultToExcludedNumbers)
                    {
                        AddExcludedNumber(result);
                    }
                    break;
                }
            }
            return result;
        }

        private uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            var buffer = new byte[bytesNumber];
            _randomNumberGenerator.GetBytes(buffer);
            return buffer;
        }

        public string GenerateRandomNumberInRangeToString(int padding, bool addResultToExcludedNumbers, out int number)
        {
            number = GenerateRandomNumberInRange(addResultToExcludedNumbers);
            string result = number.ToString().PadLeft(padding, '0');
            return result;
        }

        public void Dispose()
        {
            _randomNumberGenerator.Dispose();
        }

        #endregion //Methods
    }
}