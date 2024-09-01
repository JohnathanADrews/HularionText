#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Structure;
using HularionText.Compiler.Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HularionText.Compiler
{
    /// <summary>
    /// Defines a language.
    /// </summary>
    public abstract class HularionLanguage<ParseContext, OperationMode, LanguageDocument>
        where ParseContext : SequenceProcessContext<LanguageDocument>
    {

        /// <summary>
        /// The symbol tree.
        /// </summary>
        public TextSequenceTree SymbolTree { get; protected set; } = new TextSequenceTree();

        protected Table<SequenceTreeTextNode, OperationMode> actionTable { get; set; } = new Table<SequenceTreeTextNode, OperationMode>();
        protected Action<ParseContext> intermediateAction { get; set; } = new Action<ParseContext>(context => { });

        protected abstract ParseContext CreateContext();
        protected abstract OperationMode StartMode { get; set; }


        /// <summary>
        /// Parses the text, returning the document.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>The document.</returns>
        public LanguageDocument Parse(string text)
        {
            var context = CreateContext();
            context.SymbolTree = SymbolTree;
            context.ModeStack.Push(StartMode);
            var matches = new TextSequenceMatch[] { };
            int index = 0;

            while (index < text.Length)
            {
                matches = SymbolTree.GetNextMatches(text, index, matches.LastOrDefault(), context);
                for (var i = 0; i < matches.Length; i++)
                {
                    var match = matches[i];
                    context.Match = match;
                    if (match.IsIntermediate)
                    {
                        intermediateAction(context);
                    }
                    else if (actionTable.ContainsValue(match.Match, (OperationMode)context.Mode))
                    {
                        var action = actionTable.GetValue<Action<ParseContext>>(match.Match, (OperationMode)context.Mode);
                        action(context);
                    }
                    index = matches[i].NextIndex;
                }
            }

            return context.Document;
        }

        /// <summary>
        /// Adds the symbols to the symbol tree for the given mode.
        /// </summary>
        /// <param name="mode">The mode in which to find the symbols.</param>
        /// <param name="action">The action to take when a symbol is found.</param>
        /// <param name="symbols">The symbols to find.</param>
        protected void AddSymbolMode(OperationMode mode, Action<ParseContext> action, params TextSymbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                AddSymbolMode(mode, symbol, action);
            }
        }

        /// <summary>
        /// Adds the symbol to the symbol tree for the given modes.
        /// </summary>
        /// <param name="symbol">The symbol to add.</param>
        /// <param name="action">The action to take when the symbol is found.</param>
        /// <param name="modes">The modes in which to look for the symbol.</param>
        protected void AddSymbolMode(TextSymbol symbol, Action<ParseContext> action, params OperationMode[] modes)
        {
            foreach (var mode in modes)
            {
                AddSymbolMode(mode, symbol, action);
            }
        }

        /// <summary>
        /// Adds the symbols to the symbol tree for the given modes.
        /// </summary>
        /// <param name="symbols">The symbols to add.</param>
        /// <param name="action">The action to take when the symbol is found.</param>
        /// <param name="modes">The modes in which to look for the symbol.</param>
        protected void AddSymbolMode(IEnumerable<OperationMode> modes, IEnumerable<TextSymbol> symbols, Action<ParseContext> action)
        {
            foreach (var mode in modes)
            {
                foreach(var symbol in symbols)
                {
                    AddSymbolMode(mode, symbol, action);
                }
            }
        }

        /// <summary>
        /// Adds the symbol to the symbol tree for the given mode.
        /// </summary>
        /// <param name="mode">The symbol to add.</param>
        /// <param name="symbol">The mode in which to find the symbol.</param>
        /// <param name="action">The action to take when the symbol is found.</param>
        protected void AddSymbolMode(OperationMode mode, TextSymbol symbol, Action<ParseContext> action)
        {
            var sequenceNode = SymbolTree.AddSymbol(symbol, mode);
            actionTable.AddColumn(sequenceNode);
            actionTable.AddRow(mode);
            actionTable.SetValue(sequenceNode, mode, action);
        }
    }
}
