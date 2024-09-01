#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionText.Compiler.Sequence
{
    /// <summary>
    /// A default ISequenceProcessContext.
    /// </summary>
    public class SequenceProcessContext<DocumentType> : ISequenceProcessContext
    {
        /// <summary>
        /// The mode state of the sequence process.
        /// </summary>
        public Stack<object> ModeStack { get; private set; } = new Stack<object>();

        /// <summary>
        /// The current mode of the process from the top of ModeStack.
        /// </summary>
        public object Mode { get { return ModeStack.FirstOrDefault(); } }

        /// <summary>
        /// The current match for the context.
        /// </summary>
        public TextSequenceMatch? Match { get; set; }

        /// <summary>
        /// The symbolTree.
        /// </summary>
        public TextSequenceTree SymbolTree { get; set; }

        /// <summary>
        /// The type of document being generated.
        /// </summary>
        public DocumentType Document { get; set; }

        /// <summary>
        /// Returns the value of the match or null if the match is null.
        /// </summary>
        public string? MatchValue
        {
            get
            {
                return Match == null ? null : Match.Value.Text;
            }
        }
    }
}
