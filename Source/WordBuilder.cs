using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordgorithm
{
    /// <summary>
    /// A word builder that can be used to construct sentences etc.
    /// </summary>
    public sealed class WordBuilder
    {
        /// <summary>
        /// Collection of the current word sequence.
        /// </summary>
        private List<WordInstance> _wordSequence;

        /// <summary>
        /// The trainer data associated with the word builder.
        /// </summary>
        private WordTrainer _trainer;

        /// <summary>
        /// The current word instance.
        /// </summary>
        private WordInstance _currentWord;

        /// <summary>
        /// The minimum words of the builder.
        /// </summary>
        private int _minWords;

        /// <summary>
        /// The maximum words of the builder.
        /// </summary>
        private int _maxWords;

        /// <summary>
        /// Creates a new word builder.
        /// </summary>
        /// <param name="trainer">The trainer that holds the training data.</param>
        /// <param name="minWords">The minimum words a sentence must contain.</param>
        /// <param name="maxWords">The maximum words a sentence must contain. This is only approximate and it might still contain more words.</param>
        public WordBuilder(WordTrainer trainer, int minWords, int maxWords)
        {
            _wordSequence = new List<WordInstance>();

            _trainer = trainer;

            _minWords = minWords;
            _maxWords = maxWords;
        }

        /// <summary>
        /// Boolean determining whether the builder has reached the end of the sequence when generating.
        /// </summary>
        public bool EndOfSequence { get; private set; }
        
        /// <summary>
        /// The current symbol wrapper instance.
        /// </summary>
        private WordInstance _wrapperInstance;
        
        /// <summary>
        /// The amount of tries when building.
        /// </summary>
        private int _tries;

        /// <summary>
        /// Generates the next word or symbol in the sequence.
        /// </summary>
        /// <param name="selectUntilEndSymbol">Boolean determining whether it should select until the end symbol only.</param>
        /// <param name="includeEndSymbol">Boolean determining whether it should include the end symbol too.</param>
        /// <returns>The word builder allowing for chaining.</returns>
        public WordBuilder Next(bool selectUntilEndSymbol = true, bool includeEndSymbol = true)
        {
            if (_tries > 10)
            {
                EndOfSequence = true;
                _wordSequence.Clear();

                return this;
            }

            if (EndOfSequence)
            {
                return this;
            }

            if (_currentWord == null)
            {
                _currentWord = _trainer.SelectStartWord();

                if (_currentWord == null)
                {
                    EndOfSequence = true;

                    return this;
                }
            }
            else
            {
                _currentWord = _currentWord.Word.SelectWord();

                if (_currentWord == null)
                {
                    EndOfSequence = true;

                    return this;
                }
            }

            if (_currentWord.SymbolType == SymbolType.Wrapper && string.IsNullOrWhiteSpace(_currentWord.SymbolWrapperEnd))
            {
                _tries++;
                return Next(selectUntilEndSymbol, includeEndSymbol);
            }
            
            if (selectUntilEndSymbol && _currentWord.SymbolType == SymbolType.End)
            {
                if (includeEndSymbol || _wrapperInstance != null)
                {
                    _wordSequence.Add(_currentWord);
                }

                if (_wrapperInstance != null)
                {
                    _wordSequence.Add(_wrapperInstance);
                }

                EndOfSequence = true;
            }
            else
            {
                if (_wrapperInstance != null && _currentWord.SymbolType == SymbolType.Wrapper && _wrapperInstance.SymbolWrapperEnd == _currentWord.Word.Word)
                {
                    _wrapperInstance = null;
                }
                else if (_currentWord.SymbolType == SymbolType.Wrapper)
                {
                    _wrapperInstance = _currentWord;
                }

                _wordSequence.Add(_currentWord);
            }

            return this;
        }

        /// <summary>
        /// Converts the first character of a string to upper.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>Returns the new string.</returns>
        private static string FirstToUpper(string s)
        {
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        /// <summary>
        /// Generates the sequence.
        /// </summary>
        /// <param name="selectUntilEndSymbol">Boolean determining whether it should select until the end symbol only.</param>
        /// <param name="includeEndSymbol">Boolean determining whether it should include the end symbol too.</param>
        private void Generate(bool selectUntilEndSymbol, bool includeEndSymbol)
        {
            var brain = this;

            do
            {
                brain = Next(selectUntilEndSymbol, includeEndSymbol);
            } while (!EndOfSequence && _wordSequence.Count(w => w.SymbolType == SymbolType.None) < _maxWords);
        }

        /// <summary>
        /// Converts the sequence of the builder to a constructed string.
        /// </summary>
        /// <returns>Returns the constructed string.</returns>
        public override string ToString()
        {
            return ToString(selectUntilEndSymbol: true, includeEndSymbol: true);
        }

        /// <summary>
        /// Converts the sequence of the builder to a constructed string.
        /// </summary>
        /// <param name="selectUntilEndSymbol">Boolean determining whether it should select until the end symbol only.</param>
        /// <param name="includeEndSymbol">Boolean determining whether it should include the end symbol too.</param>
        /// <returns>Returns the constructed string.</returns>
        public string ToString(bool selectUntilEndSymbol, bool includeEndSymbol)
        {
            if (_tries > 10)
            {
                return null;
            }

            if (_wordSequence.Count(w => w.SymbolType == SymbolType.None) > _maxWords)
            {
                _maxWords = Math.Min(_wordSequence.Count, _maxWords);

                for (var i = 0; i < _maxWords; i++)
                {
                    var word = _wordSequence[i];
                    var nextWord = i < (_maxWords - 1) ? _wordSequence[i + 1] : null;

                    if (word.SymbolType == SymbolType.Wrapper && string.IsNullOrWhiteSpace(word.SymbolWrapperEnd))
                    {
                        _wordSequence = _wordSequence.Take(i + 1).ToList();
                        break;
                    }
                    else if (word.SymbolType == SymbolType.End && (nextWord == null || !(nextWord.SymbolType == SymbolType.Wrapper && string.IsNullOrWhiteSpace(nextWord.SymbolWrapperEnd))))
                    {
                        _wordSequence = _wordSequence.Take(i + 1).ToList();
                        break;
                    }
                }
            }

            if (_wordSequence.Count(w => w.SymbolType == SymbolType.None) < _minWords)
            {
                _wordSequence.Clear();
                EndOfSequence = false;
                _currentWord = null;
                _wrapperInstance = null;

                Generate(selectUntilEndSymbol, includeEndSymbol);

                _tries++;

                return ToString(selectUntilEndSymbol, includeEndSymbol);
            }

            var result = new StringBuilder();
            
            for (var i = 0; i < _wordSequence.Count; i++)
            {
                var word = _wordSequence[i];
                var nextWord = i < (_wordSequence.Count - 1) ? _wordSequence[i + 1] : null;

                switch (word.SymbolType)
                {
                    case SymbolType.Spacing: result.AppendFormat(" {0} ", word.Word.Word); break;

                    case SymbolType.Combinator:
                    case SymbolType.Separator:
                    case SymbolType.Wrapper:
                        result.AppendFormat("{0}", word.Word.Word);
                        break;
                    
                    case SymbolType.End:
                    case SymbolType.EndSeparator:
                    case SymbolType.None:
                    default:
                        if (nextWord != null && (nextWord.SymbolType == SymbolType.End || nextWord.SymbolType == SymbolType.EndSeparator || nextWord.SymbolType == SymbolType.Combinator))
                        {
                            result.AppendFormat("{0}", word.Word.Word);
                        }
                        else if (nextWord != null && nextWord.SymbolType == SymbolType.Wrapper && word.SymbolType == SymbolType.End)
                        {
                            result.AppendFormat("{0}", word.Word.Word);
                        }
                        else
                        {
                            result.AppendFormat("{0} ", word.Word.Word);
                        }
                        break;
                }
            }

            return FirstToUpper(result.ToString().Trim());
        }
    }
}
