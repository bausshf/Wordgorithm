using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordgorithm
{
    /// <summary>
    ///  A training word.
    /// </summary>
    internal class TrainingWord
    {
        /// <summary>
        /// The next id of a training word.
        /// </summary>
        private static ulong _nextId;

        /// <summary>
        /// The id of the training word.
        /// </summary>
        private ulong _id;

        /// <summary>
        /// Createsa  new training word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="defaultEndWord">The default end word for the training word.</param>
        internal TrainingWord(string word, WordInstance defaultEndWord)
        {
            Word = word;
            DefaultEndWord = defaultEndWord;

            PostWords = new Dictionary<string, WordInstance>();

            _id = _nextId;
            _nextId++;
        }

        internal ulong Id { get { return _id; } }

        /// <summary>
        /// Gets the raw word.
        /// </summary>
        internal string Word { get; private set; }

        /// <summary>
        /// Gets all the post words of the training word.
        /// </summary>
        internal Dictionary<string, WordInstance> PostWords { get; }

        /// <summary>
        /// Gets the default end word of the training word.
        /// </summary>
        internal WordInstance DefaultEndWord { get; }

        /// <summary>
        /// Selects a random word within the first 10000 heaviest connected words of the training word.
        /// </summary>
        /// <returns>The word instance if found, the default end word otherwise.</returns>
        internal WordInstance SelectWord()
        {
            var sortedWords = PostWords.OrderByDescending(td => td.Value.Weight).Take(Math.Min(PostWords.Count, 10000)).ToList();

            if (sortedWords.Count == 0)
            {
                return DefaultEndWord;
            }

            return sortedWords[WordRand.Next(0, sortedWords.Count)].Value;
        }
    }
}
