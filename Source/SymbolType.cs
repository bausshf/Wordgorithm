using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordgorithm
{
    /// <summary>
    /// Enumeration of symbol types.
    /// </summary>
    public enum SymbolType : int
    {
        /// <summary>
        /// A word and not a symbol.
        /// </summary>
        None = 0,
        /// <summary>
        /// An ending symbol.
        /// </summary>
        End = 1,
        /// <summary>
        /// An ending symbol for separators.
        /// </summary>
        EndSeparator = 2,
        /// <summary>
        /// A spacing symbol.
        /// </summary>
        Spacing = 3,
        /// <summary>
        /// A separator symbol.
        /// </summary>
        Separator = 4,
        /// <summary>
        /// A combinator symbol.
        /// </summary>
        Combinator = 5,
        /// <summary>
        /// A wrapper.
        /// </summary>
        Wrapper = 6
    }
}
