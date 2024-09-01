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
    /// Contains the matches in Sequence with respect to the sequence tree or an intermediate value.
    /// </summary>
    public class TextSequenceMatch
    {
        /// <summary>
        /// The tree used to obtain the matches.
        /// </summary>
        public TextSequenceTree Tree { get; set; }

        /// <summary>
        /// The sequence of objects to match.
        /// </summary>
        public string Sequence { get; set; }

        /// <summary>
        /// The index at which the matches start in Sequence .
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// The nodes of Tree that have matching sequences in Sequence.  The node with the longest sequence is on top.
        /// </summary>
        public Stack<SequenceTreeTextNode> Matches { get; set; } = new Stack<SequenceTreeTextNode>();


        /// <summary>
        /// The longest match if there is one.
        /// </summary>
        public SequenceTreeTextNode Match { get { return HasMatch ? Matches.Peek() : null; } }

        /// <summary>
        /// True iff this represents a range in Sequence between matches.
        /// </summary>
        public bool IsIntermediate { get; set; } = false;

        /// <summary>
        /// True iff there is at least one match in Matches.
        /// </summary>
        public bool HasMatch { get { return Matches.Count > 0; } }

        /// <summary>
        /// Extends the Length offset by the indicated amount.
        /// </summary>
        public int Extend { get; set; } = 0;

        /// <summary>
        /// Returns the length of the top-most match or the set length if an intermediate.
        /// </summary>
        public int Length 
        { 
            get 
            { 
                if (IsIntermediate) 
                { 
                    return length + Extend; 
                } 
                if (HasMatch) 
                { 
                    return Matches.Peek().Symbol.Length + Extend; 
                } 
                return 0; 
            } 
            set { length = value; } }

        /// <summary>
        /// Returns the next index to examine.
        /// </summary>
        public int NextIndex { get { return StartIndex + Length; } }

        /// <summary>
        /// The matching items in the sequence.
        /// </summary>
        public TextSymbol Value
        {
            get
            {
                TextSymbol value;
                if (IsIntermediate)
                {
                    value = new TextSymbol();
                    value.Text = Sequence.Substring(StartIndex, NextIndex - StartIndex);
                }
                else { value = Match.Symbol; }
                return value;
            }
        }

        private int length { get; set; } = 0;

        /// <summary>
        /// Pushes a tree node as a match.
        /// </summary>
        /// <param name="node">The node to push.</param>
        public void Push(SequenceTreeTextNode node)
        {
            Matches.Push(node);
        }

        /// <summary>
        /// Pops the top match if there is at least one match.
        /// </summary>
        public void Pop()
        {
            if (HasMatch) { Matches.Pop(); }
        }

        public static TextSequenceMatch CreateIntermediate(TextSequenceTree tree, string sequence, int startIndex, int length)
        {
           
            return new TextSequenceMatch() { Tree = tree, Sequence = sequence, StartIndex = startIndex, Length = length, IsIntermediate = true };
        }

    }
}
