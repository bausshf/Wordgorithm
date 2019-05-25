using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordgorithm
{
    /// <summary>
    /// The word trainer.
    /// </summary>
    public sealed class WordTrainer
    {
        /// <summary>
        /// The end symbols.
        /// </summary>
        private IReadOnlyCollection<string> _endSymbols;
        
        /// <summary>
        /// The end separator symbols.
        /// </summary>
        private IReadOnlyCollection<string> _endSeparatorSymbols;

        /// <summary>
        /// The spacing symbols.
        /// </summary>
        private IReadOnlyCollection<string> _spacingSymbols;

        /// <summary>
        /// The separator symbols.
        /// </summary>
        private IReadOnlyCollection<string> _separatorSymbols;

        /// <summary>
        /// The combinator symbols.
        /// </summary>
        private IReadOnlyCollection<string> _combinatorSymbols;

        /// <summary>
        ///  The wrapper symbols.
        /// </summary>
        private IReadOnlyList<string> _wrapperSymbols;

        /// <summary>
        /// The symbol hashes.
        /// </summary>
        private HashSet<string> _symbolHashes;

        /// <summary>
        ///  The symbol wrapper ends.
        /// </summary>
        private Dictionary<string, string> _wrapperEnds;

        /// <summary>
        /// The training data.
        /// </summary>
        private Dictionary<string, WordInstance> _trainingData;

        /// <summary>
        /// The default end symbol.
        /// </summary>
        private WordInstance _defaultEndSymbol;

        /// <summary>
        /// Creates a new word trainer.
        /// </summary>
        public WordTrainer()
            : this(".", new[] { ".", "?", "!" }, new[] { ",", ":", ";" }, new[] { "|", "/", "&" }, new[] { ">", "<", "~", "^", "*" }, new[] { "@" }, new[] { "\"", "\"", "(", ")", "[", "]", "“", "”" })
        {

        }

        /// <summary>
        /// Creates a new word trainer.
        /// </summary>
        /// <param name="defaultEndSymbol">The default end symbol.</param>
        /// <param name="endSymbols">The end symbols.</param>
        /// <param name="endSeparators">The end separators.</param>
        /// <param name="spacingSymbols">The spacing symbols.</param>
        /// <param name="separatorSymbols">The separator symbols.</param>
        /// <param name="combinatorSymbols">The combinator symbols.</param>
        /// <param name="wrapperSymbols">The wrapper symbols.</param>
        public WordTrainer(string defaultEndSymbol, IReadOnlyCollection<string> endSymbols, IReadOnlyCollection<string> endSeparators, IReadOnlyCollection<string> spacingSymbols, IReadOnlyCollection<string> separatorSymbols, IReadOnlyCollection<string> combinatorSymbols, IReadOnlyList<string> wrapperSymbols)
        {
            _trainingData = new Dictionary<string, WordInstance>();
            _wrapperEnds = new Dictionary<string, string>();
            _symbolHashes = new HashSet<string>();

            _endSymbols = endSymbols;
            _endSeparatorSymbols = endSeparators;
            _spacingSymbols = spacingSymbols;
            _separatorSymbols = separatorSymbols;
            _combinatorSymbols = combinatorSymbols;
            _wrapperSymbols = wrapperSymbols;

            if (!string.IsNullOrWhiteSpace(defaultEndSymbol))
            {
                _defaultEndSymbol = new WordInstance(1, new TrainingWord(defaultEndSymbol, null), SymbolType.End);
            }

            for (var i = 0; i < wrapperSymbols.Count; i++)
            {
                var a = _wrapperSymbols[i];
                i++;
                var b = _wrapperSymbols[i];
                _wrapperEnds.Add(a,b);
            }

            foreach (var symbol in _endSymbols)
            {
                _symbolHashes.Add(symbol);
            }
            foreach (var symbol in _endSeparatorSymbols)
            {
                _symbolHashes.Add(symbol);
            }
            foreach (var symbol in _spacingSymbols)
            {
                _symbolHashes.Add(symbol);
            }
            foreach (var symbol in _separatorSymbols)
            {
                _symbolHashes.Add(symbol);
            }
            foreach (var symbol in _combinatorSymbols)
            {
                _symbolHashes.Add(symbol);
            }
            foreach (var symbol in _wrapperSymbols)
            {
                _symbolHashes.Add(symbol);
            }
        }

        /// <summary>
        /// Creates a word builder based on the training data of this trainer.
        /// </summary>
        /// <param name="minWords">The minimum words of the sequence generated.</param>
        /// <param name="maxWords">The maximum words of the sequence generated. This is only an approximate value since it could still generate more words in certain cases.</param>
        /// <returns>The word builder.</returns>
        public WordBuilder CreateBuilder(int minWords, int maxWords)
        {
            return new WordBuilder(this, minWords, maxWords);
        }

        /// <summary>
        /// Selects a starting word for a sentence.
        /// </summary>
        /// <returns>The word instance.</returns>
        internal WordInstance SelectStartWord()
        {
            var sortedWords = _trainingData.Where(w => !_symbolHashes.Contains(w.Key) || w.Value.SymbolType == SymbolType.Wrapper).OrderByDescending(td => td.Value.Weight).Take(Math.Min(_trainingData.Count, 10000)).ToList();

            return sortedWords[WordRand.Next(0, sortedWords.Count)].Value;
        }

        /// <summary>
        /// Adds training data.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="levels">The levels of training. This is 1 by default. More levels = larger weight of each word/symbol.</param>
        public void Train(IEnumerable<string> input, int levels = 1)
        {
            foreach (var i in input)
            {
                Train(i, levels);
            }
        }

        /// <summary>
        /// Adds training data.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="levels">The levels of training. This is 1 by default. More levels = larger weight of each word/symbol.</param>
        public void Train(string input, int levels = 1)
        {
            for (var i = 0; i < levels; i++)
            {
                var data = input.Trim();

                foreach (var symbol in _endSymbols)
                {
                    data = data.Replace(symbol, $" {symbol} ");
                }

                foreach (var symbol in _spacingSymbols)
                {
                    data = data.Replace(symbol, $" {symbol} ");
                }

                foreach (var symbol in _separatorSymbols)
                {
                    data = data.Replace(symbol, $" {symbol} ");
                }

                foreach (var symbol in _combinatorSymbols)
                {
                    data = data.Replace(symbol, $" {symbol} ");
                }

                foreach (var symbol in _wrapperSymbols)
                {
                    data = data.Replace(symbol, $" {symbol} ");
                }

                var words = data.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList();

                WordInstance lastWord = null;
                foreach (var word in words)
                {
                    WordInstance trainingWord;
                    if (!_trainingData.TryGetValue(word, out trainingWord))
                    {
                        var symbolType = SymbolType.None;
                        string symbolWrapperEnd = null;

                        if (_endSymbols.Contains(word))
                        {
                            symbolType = SymbolType.End;
                        }
                        else if (_endSeparatorSymbols.Contains(word))
                        {
                            symbolType = SymbolType.EndSeparator;
                        }
                        else if (_spacingSymbols.Contains(word))
                        {
                            symbolType = SymbolType.Spacing;
                        }
                        else if (_separatorSymbols.Contains(word))
                        {
                            symbolType = SymbolType.Separator;
                        }
                        else if (_combinatorSymbols.Contains(word))
                        {
                            symbolType = SymbolType.Combinator;
                        }
                        else if (_wrapperSymbols.Contains(word))
                        {
                            symbolType = SymbolType.Wrapper;

                            _wrapperEnds.TryGetValue(word, out symbolWrapperEnd);
                        }

                        trainingWord = new WordInstance(1, new TrainingWord(word, _defaultEndSymbol), symbolType, symbolWrapperEnd);

                        _trainingData.Add(word, trainingWord);
                    }
                    else if (trainingWord != null)
                    {
                        trainingWord.IncreaseWeight();
                    }

                    if (lastWord != null && lastWord.Word != null)
                    {
                        WordInstance preWordInstance;

                        if (!lastWord.Word.PostWords.TryGetValue(word, out preWordInstance))
                        {
                            lastWord.Word.PostWords.Add(word, new WordInstance(1, trainingWord.Word, trainingWord.SymbolType));
                        }
                        else if (preWordInstance != null)
                        {
                            preWordInstance.IncreaseWeight();
                        }
                    }

                    lastWord = trainingWord;
                }
            }
        }
    }
}
