using System;
using System.Collections.Generic;
using System.IO;
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

        public void LoadModel(string modelFile)
        {
            if (!File.Exists(modelFile))
            {
                return;
            }

            using (var fs = new FileStream(modelFile, FileMode.Open))
            {
                using (var br = new BinaryReader(fs))
                {
                    // Symbols
                    var endSymbolsCount = br.ReadUInt64();
                    var endSeparatorSymbolsCount = br.ReadUInt64();
                    var spacingSymbolsCount = br.ReadUInt64();
                    var separatorSymbolsCount = br.ReadUInt64();
                    var combinatorSymbolsCount = br.ReadUInt64();
                    var wrapperSymbolsCount = br.ReadUInt64();

                    var endSymbols = new List<string>();

                    for (uint i = 0; i < endSymbolsCount; i++)
                    {
                        var symbolLength = br.ReadUInt32();
                        var buffer = br.ReadBytes((int)symbolLength);

                        endSymbols.Add(Encoding.Unicode.GetString(buffer));
                    }

                    var endSeparatorSymbols = new List<string>();

                    for (uint i = 0; i < endSeparatorSymbolsCount; i++)
                    {
                        var symbolLength = br.ReadUInt32();
                        var buffer = br.ReadBytes((int)symbolLength);

                        endSeparatorSymbols.Add(Encoding.Unicode.GetString(buffer));
                    }

                    var spacingSymbols = new List<string>();

                    for (uint i = 0; i < spacingSymbolsCount; i++)
                    {
                        var symbolLength = br.ReadUInt32();
                        var buffer = br.ReadBytes((int)symbolLength);

                        spacingSymbols.Add(Encoding.Unicode.GetString(buffer));
                    }

                    var separatorSymbols = new List<string>();

                    for (uint i = 0; i < separatorSymbolsCount; i++)
                    {
                        var symbolLength = br.ReadUInt32();
                        var buffer = br.ReadBytes((int)symbolLength);

                        separatorSymbols.Add(Encoding.Unicode.GetString(buffer));
                    }

                    var combinatorSymbols = new List<string>();

                    for (uint i = 0; i < combinatorSymbolsCount; i++)
                    {
                        var symbolLength = br.ReadUInt32();
                        var buffer = br.ReadBytes((int)symbolLength);

                        combinatorSymbols.Add(Encoding.Unicode.GetString(buffer));
                    }

                    var wrapperSymbols = new List<string>();

                    for (uint i = 0; i < wrapperSymbolsCount; i++)
                    {
                        var symbolLength = br.ReadUInt32();
                        var buffer = br.ReadBytes((int)symbolLength);

                        wrapperSymbols.Add(Encoding.Unicode.GetString(buffer));
                    }

                    // Training Data
                    var trainingDataCount = br.ReadUInt64();

                    var trainingData = new Dictionary<ulong, WordInstance>();

                    var trainingIds = new Dictionary<ulong, List<ulong>>();

                    for (ulong i = 0; i < trainingDataCount; i++)
                    {
                        var weight = br.ReadInt32();
                        var symbolType = (SymbolType)br.ReadInt32();
                        var hasSymbolWrapperEnd = br.ReadByte() == 1;
                        string symbolWrapperEnd = null;

                        if (hasSymbolWrapperEnd)
                        {
                            var symbolWrapperEndCount = br.ReadUInt32();
                            var symbolWrapperBuffer = br.ReadBytes((int)symbolWrapperEndCount);

                            symbolWrapperEnd = Encoding.Unicode.GetString(symbolWrapperBuffer);
                        }

                        var wordId = br.ReadUInt64();

                        var wordLength = br.ReadUInt32();
                        var wordBuffer = br.ReadBytes((int)wordLength);
                        var word = symbolWrapperEnd = Encoding.Unicode.GetString(wordBuffer);

                        var postWordCount = br.ReadUInt64();

                        var postWordIds = new List<ulong>();

                        for (ulong j = 0; j < postWordCount; j++)
                        {
                            postWordIds.Add(br.ReadUInt64());
                        }
                        
                        var trainingWord = new WordInstance(weight, new TrainingWord(word, _defaultEndSymbol), symbolType, symbolWrapperEnd);

                        trainingData.Add(wordId, trainingWord);
                        trainingIds.Add(wordId, postWordIds);
                    }

                    foreach (var instance in trainingData)
                    {
                        var ids = trainingIds[instance.Key];

                        foreach (var id in ids)
                        {
                            WordInstance wordInstance;
                            if (trainingData.TryGetValue(id, out wordInstance))
                            {
                                instance.Value.Word.PostWords.Add(wordInstance.Word.Word, wordInstance);
                            }
                        }

                        _trainingData.Add(instance.Value.Word.Word, instance.Value);
                    }
                }
            }
        }

        public void SaveModel(string modelFile)
        {
            if (File.Exists(modelFile))
            {
                if (File.Exists("__backupModel.dat"))
                {
                    File.Delete("__backupModel.dat");
                }

                File.Copy(modelFile, "__backupModel.dat");
                File.Delete(modelFile);
            }

            using (var fs = new FileStream(modelFile, FileMode.CreateNew))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    // Symbols
                    bw.Write((ulong)_endSymbols.Count);
                    bw.Write((ulong)_endSeparatorSymbols.Count);
                    bw.Write((ulong)_spacingSymbols.Count);
                    bw.Write((ulong)_separatorSymbols.Count);
                    bw.Write((ulong)_combinatorSymbols.Count);
                    bw.Write((ulong)_wrapperSymbols.Count);
                    
                    foreach (var symbol in _endSymbols)
                    {
                        var buffer = Encoding.Unicode.GetBytes(symbol);
                        bw.Write((uint)buffer.Length);
                        bw.Write(buffer);
                    }

                    foreach (var symbol in _endSeparatorSymbols)
                    {
                        var buffer = Encoding.Unicode.GetBytes(symbol);
                        bw.Write((uint)buffer.Length);
                        bw.Write(buffer);
                    }

                    foreach (var symbol in _spacingSymbols)
                    {
                        var buffer = Encoding.Unicode.GetBytes(symbol);
                        bw.Write((uint)buffer.Length);
                        bw.Write(buffer);
                    }

                    foreach (var symbol in _separatorSymbols)
                    {
                        var buffer = Encoding.Unicode.GetBytes(symbol);
                        bw.Write((uint)buffer.Length);
                        bw.Write(buffer);
                    }

                    foreach (var symbol in _combinatorSymbols)
                    {
                        var buffer = Encoding.Unicode.GetBytes(symbol);
                        bw.Write((uint)buffer.Length);
                        bw.Write(buffer);
                    }

                    foreach (var symbol in _wrapperSymbols)
                    {
                        var buffer = Encoding.Unicode.GetBytes(symbol);
                        bw.Write((uint)buffer.Length);
                        bw.Write(buffer);
                    }

                    // Training Data
                    bw.Write((ulong)_trainingData.Count);

                    foreach (var word in _trainingData.Values)
                    {
                        bw.Write((int)word.Weight);
                        bw.Write((int)word.SymbolType);
                        bw.Write((byte)(!string.IsNullOrWhiteSpace(word.SymbolWrapperEnd) ? 1 : 0));

                        if (!string.IsNullOrWhiteSpace(word.SymbolWrapperEnd))
                        {
                            var symbolBuffer = Encoding.Unicode.GetBytes(word.SymbolWrapperEnd);
                            bw.Write((uint)symbolBuffer.Length);
                            bw.Write(symbolBuffer);
                        }

                        bw.Write((ulong)word.Word.Id);

                        var buffer = Encoding.Unicode.GetBytes(word.Word.Word);
                        bw.Write((uint)buffer.Length);
                        bw.Write(buffer);

                        bw.Write((ulong)word.Word.PostWords.Count);

                        foreach (var postWord in word.Word.PostWords.Values)
                        {
                            bw.Write((ulong)postWord.Word.Id);
                        }
                    }
                }
            }
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
