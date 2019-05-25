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
    public enum SymbolType
    {
        /// <summary>
        /// A word and not a symbol.
        /// </summary>
        None,
        /// <summary>
        /// An ending symbol.
        /// </summary>
        End,
        /// <summary>
        /// An ending symbol for separators.
        /// </summary>
        EndSeparator,
        /// <summary>
        /// A spacing symbol.
        /// </summary>
        Spacing,
        /// <summary>
        /// A separator symbol.
        /// </summary>
        Separator,
        /// <summary>
        /// A combinator symbol.
        /// </summary>
        Combinator,
        /// <summary>
        /// A wrapper.
        /// </summary>
        Wrapper
    }
}
