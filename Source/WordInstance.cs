using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordgorithm
{
    /// <summary>
    /// A word instance.
    /// </summary>
    internal class WordInstance
    {
        /// <summary>
        /// Creates a new word instance.
        /// </summary>
        /// <param name="initialWeight">The initial weight of the word.</param>
        /// <param name="word">The word.</param>
        /// <param name="symbolType">The symbol type.</param>
        /// <param name="symbolWrapperEnd">The symbol wrapper end word.</param>
        internal WordInstance(int initialWeight, TrainingWord word, SymbolType symbolType, string symbolWrapperEnd = null)
        {
            Weight = Math.Min(100, Math.Max(0, initialWeight));
            Word = word;
            SymbolType = symbolType;
            SymbolWrapperEnd = symbolWrapperEnd;
        }

        /// <summary>
        ///  Gets the weight of the word.
        /// </summary>
        internal int Weight { get; private set; }

        /// <summary>
        ///  Gets the training word associated with the word.
        /// </summary>
        internal TrainingWord Word { get; private set; }

        /// <summary>
        ///  Gets the symbol type.
        /// </summary>
        internal SymbolType SymbolType { get; private set; }

        /// <summary>
        ///  Gets the symbol wrapper end word.
        /// </summary>
        internal string SymbolWrapperEnd { get; private set; }

        /// <summary>
        /// Increases the weight of the word by one.
        /// </summary>
        internal void IncreaseWeight()
        {
            if (Weight == int.MaxValue)
            {
                return;
            }

            Weight++;
        }
    }
}
