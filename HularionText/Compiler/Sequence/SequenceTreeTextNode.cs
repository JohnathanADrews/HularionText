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
using System.Text;

namespace HularionText.Compiler.Sequence
{
    /// <summary>
    /// A node of a sequence tree.
    /// </summary>
    /// <typeparam name="SequenceType">The type of the objects which are ordered.  e.g. a block of characters in text.</typeparam>
    /// <typeparam name="CompositeType">Represents the composite details of a block of symbols.</typeparam>
    public class SequenceTreeTextNode
    {
        /// <summary>
        /// The tree to which this node belongs.
        /// </summary>
        public TextSequenceTree Tree { get; set; }

        /// <summary>
        /// True iff this is the root node.
        /// </summary>
        public bool IsRoot { get; set; } = false;

        /// <summary>
        /// The value this node represents in the tree.
        /// </summary>
        public char Value { get; set; }

        /// <summary>
        /// The symbol to which this node belongs.
        /// </summary>
        public TextSymbol Symbol { get; set; }

        /// <summary>
        /// True iff this node represents a complete symbol in the tree.
        /// </summary>
        public bool IsWaypoint { get; set; } = false;

        /// <summary>
        /// The next nodes in the tree.
        /// </summary>
        public IDictionary<char, SequenceTreeTextNode> Nodes = new Dictionary<char, SequenceTreeTextNode>();

        /// <summary>
        /// The modes for which this node is applicable. 
        /// </summary>
        public HashSet<object> SequenceModes { get; set; } = new HashSet<object>();

        /// <summary>
        /// Gets the next node given the sequence value. 
        /// </summary>
        /// <param name="item">The locator for the next node.</param>
        /// <returns>The next node in the tree.</returns>
        public SequenceTreeTextNode Next(char item)
        {
            var resolved = Tree.ValueResolver(item);
            if (Nodes.ContainsKey(resolved)) { return Nodes[resolved]; }
            return null;
        }
    }
}
