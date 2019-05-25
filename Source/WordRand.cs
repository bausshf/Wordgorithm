using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordgorithm
{
    /// <summary>
    /// Static random generator.
    /// </summary>
    internal static class WordRand
    {
        /// <summary>
        /// The random generator.
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Gets the next random number.
        /// </summary>
        /// <param name="min">Minimum number.</param>
        /// <param name="max">Max number.</param>
        /// <returns>The next random number.</returns>
        internal static int Next(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}
