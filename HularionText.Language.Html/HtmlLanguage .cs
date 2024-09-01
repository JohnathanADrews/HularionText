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
using HularionText.Compiler;
using HularionText.Compiler.Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionText.Language.Html
{
    /// <summary>
    /// A language parser for HTML that can convert HTML string into a HtmlDocument.
    /// </summary>
    public class HtmlLanguage
    {

        public TextSequenceTree SymbolTree { get; set; } = new TextSequenceTree() { Name = "Html Symbol Tree" };

        TextSymbol openNodeBegin = new TextSymbol() { Text = "<" };
        TextSymbol openNodeEnd = new TextSymbol() { Text = "</" };
        TextSymbol closeTag = new TextSymbol() { Text = ">" };
        TextSymbol selfCloseNode = new TextSymbol() { Text = "/>" };
        TextSymbol startComment = new TextSymbol() { Text = "<!--" };
        TextSymbol endComment = new TextSymbol() { Text = "-->" };

        TextSymbol doubleQuote = new TextSymbol() { Text = "\"" };
        TextSymbol singleQuote = new TextSymbol() { Text = "'" };
        TextSymbol operatorEquals = new TextSymbol() { Text = "=" };
        TextSymbol carriageReturn = new TextSymbol() { Text = "\r" };
        TextSymbol newline = new TextSymbol() { Text = "\n" };
        TextSymbol space = new TextSymbol() { Text = " " };
        TextSymbol tab = new TextSymbol() { Text = "\t" };

        Table<SequenceTreeTextNode, HtmlOperationMode> actionTable = new Table<SequenceTreeTextNode, HtmlOperationMode>();
        Action<HtmlContext> intermediateAction = new Action<HtmlContext>(context => { });

        HtmlOperationMode startMode = new HtmlOperationMode() { Name = "Start" };
        HtmlOperationMode tagOpenMode = new HtmlOperationMode() { Name = "Tag Open" };
        HtmlOperationMode nodeEndMode = new HtmlOperationMode() { Name = "End Node" };
        HtmlOperationMode tagContentMode = new HtmlOperationMode() { Name = "Tag Content" };
        HtmlOperationMode nodeContentMode = new HtmlOperationMode() { Name = "Node Content" };
        HtmlOperationMode nodeTextContentMode = new HtmlOperationMode() { Name = "Node Text Content" };
        HtmlOperationMode nodeClosedMode = new HtmlOperationMode() { Name = "Node Closed" };
        HtmlOperationMode attributeValueMode = new HtmlOperationMode() { Name = "Attribute Value Mode" };
        HtmlOperationMode attributeSingleQuoteMode = new HtmlOperationMode() { Name = "Attribute Single Quote" };
        HtmlOperationMode attributeDoubleQuoteMode = new HtmlOperationMode() { Name = "Attribute Double Quote" };
        HtmlOperationMode commentMode = new HtmlOperationMode() { Name = "Comment" };

        private char[] whitespace = new char[] { ' ', '\t', '\r', '\n', (char)65279 };

        public HtmlLanguage()
        {
            intermediateAction = new Action<HtmlContext>(context =>
            {
                if (context.Mode == startMode || context.Mode == nodeContentMode)
                {
                    var node = new HtmlNode() { Element = HtmlElement.FreeText };
                    node.InnerText = context.Text.Substring(context.ValueStartIndex, context.Match.Length).Trim(whitespace);
                    if (!String.IsNullOrWhiteSpace(node.InnerText))
                    {
                        context.Node.AddNode(node);
                    }
                    return;
                }
                if (context.Mode == tagOpenMode)
                {
                    context.SetNodeElementFromSubstring(context.ValueStartIndex, context.Match.NextIndex);
                    return;
                }
                if (context.Mode == tagContentMode)
                {
                    var text = context.Text.Substring(context.ValueStartIndex, context.Match.Length);
                    if (!String.IsNullOrWhiteSpace(text))
                    {
                        context.Attribute = new HtmlAttribute() { Name = text, Node = context.Node, HasEquals = false, QuoteType = AttributeQuoteType.NoQuote };
                        context.Node.Attributes.Add(context.Attribute);
                        context.Document.AddAttributes(context.Attribute);
                    }
                    return;
                }
            });

            #region NewNode
            AddSymbolModes(openNodeBegin, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Push(tagOpenMode);
                var node = new HtmlNode() { Parent = context.Node };
                context.Node.AddNode(node);
                context.Node = node;
                context.ValueStartIndex = context.Match.NextIndex;
            }), startMode, nodeContentMode, nodeClosedMode);

            AddSymbolModes(closeTag, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                if (context.Node.Element.IsTextContainer)
                {
                    context.ModeStack.Push(nodeTextContentMode);
                }
                else
                {
                    context.ModeStack.Push(nodeContentMode);
                }
                context.ValueStartIndex = context.Match.NextIndex;
            }), tagOpenMode, tagContentMode);

            AddSymbolModes(selfCloseNode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Node = context.Node.Parent;
                context.ValueStartIndex = context.Match.NextIndex;
            }), tagOpenMode, tagContentMode);

            AddModeSymbols(tagOpenMode, new Action<HtmlContext>(context =>
            {
                context.ValueStartIndex = context.Match.NextIndex;
                context.ModeStack.Pop();
                context.ModeStack.Push(tagContentMode);
            }), newline, carriageReturn, space, tab);

            #endregion

            #region CloseNode
            AddSymbolMode(nodeContentMode, openNodeEnd, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Push(nodeEndMode);
                context.ValueStartIndex = context.Match.NextIndex;
            }));
            AddModeSymbols(nodeTextContentMode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Push(nodeEndMode);
                context.Node.InnerText = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.ValueStartIndex = context.Match.NextIndex;
            }), openNodeEnd);
            AddSymbolMode(nodeEndMode, closeTag, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Node = context.Node.Parent;
                context.ValueStartIndex = context.Match.NextIndex;
            }));
            #endregion

            #region Attributes

            AddModeSymbols(tagContentMode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Push(attributeValueMode);
                context.ValueStartIndex = context.Match.NextIndex;
                context.Attribute.HasEquals = true;
            }), operatorEquals);

            AddModeSymbols(attributeValueMode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Push(attributeDoubleQuoteMode);
                context.ValueStartIndex = context.Match.NextIndex;
                context.Attribute.QuoteType = AttributeQuoteType.Double;
            }), doubleQuote);

            AddModeSymbols(attributeValueMode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Push(attributeSingleQuoteMode);
                context.ValueStartIndex = context.Match.NextIndex;
                context.Attribute.QuoteType = AttributeQuoteType.Single;
            }), singleQuote);

            AddModeSymbols(attributeValueMode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Attribute.Value = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.Attribute.QuoteType = AttributeQuoteType.NoQuote;
                context.Attribute.HasEquals = true;
                context.ValueStartIndex = context.Match.NextIndex;
            }), space, tab, newline, carriageReturn);

            AddModeSymbols(attributeValueMode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Pop();
                context.ModeStack.Push(nodeContentMode);
                context.Attribute.Value = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.Attribute.QuoteType = AttributeQuoteType.NoQuote;
                context.Attribute.HasEquals = true;
                context.ValueStartIndex = context.Match.NextIndex;
            }), closeTag);

            AddModeSymbols(attributeDoubleQuoteMode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Attribute.Value = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.Attribute.QuoteType = AttributeQuoteType.Double;
            }), doubleQuote);

            AddModeSymbols(attributeSingleQuoteMode, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Attribute.Value = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.Attribute.QuoteType = AttributeQuoteType.Single;
            }), singleQuote);

            AddModeSymbols(tagContentMode, new Action<HtmlContext>(context =>
            {
                context.ValueStartIndex = context.Match.NextIndex;
            }), space, tab, newline, carriageReturn);


            #endregion

            #region Comments

            AddSymbolModes(startComment, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Push(commentMode);
            }), startMode, nodeContentMode);

            AddSymbolModes(endComment, new Action<HtmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ValueStartIndex = context.Match.NextIndex;
            }), commentMode);

            #endregion
        }

        /// <summary>
        /// Parses the html text, creating a HtmlDocument.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>A HtmlDocument.</returns>
        public HtmlDocument Parse(string text)
        {
            var context = new HtmlContext() { TextTree = SymbolTree, Text = text, Document = new HtmlDocument(), Node = new HtmlNode() };
            context.Document.Root = context.Node;
            context.ModeStack.Push(startMode);
            var matches = new TextSequenceMatch[] { };
            int index = 0;

            while (index < text.Length)
            {
                matches = SymbolTree.GetNextMatches(text, index, matches.LastOrDefault(), context);
                for (var i = 0; i < matches.Length; i++)
                {
                    var match = matches[i];
                    context.Match = match;
                    var symbolText = match.Value.Text;
                    if (match.IsIntermediate)
                    {
                        intermediateAction(context);
                    }
                    else if (actionTable.ContainsValue(match.Match, (HtmlOperationMode)context.Mode))
                    {
                        var action = actionTable.GetValue<Action<HtmlContext>>(match.Match, (HtmlOperationMode)context.Mode);
                        action(context);
                    }
                    index = matches[i].NextIndex;
                }
            }

            return context.Document;
        }



        private void AddModeSymbols(HtmlOperationMode mode, Action<HtmlContext> action, params TextSymbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                AddSymbolMode(mode, symbol, action);
            }
        }

        private void AddSymbolModes(TextSymbol symbol, Action<HtmlContext> action, params HtmlOperationMode[] modes)
        {
            foreach (var mode in modes)
            {
                AddSymbolMode(mode, symbol, action);
            }
        }

        private void AddSymbolMode(HtmlOperationMode mode, TextSymbol symbol, Action<HtmlContext> action)
        {
            var sequenceNode = SymbolTree.AddSymbol(symbol, mode);
            actionTable.AddColumn(sequenceNode);
            actionTable.AddRow(mode);
            actionTable.SetValue(sequenceNode, mode, action);
        }



    }
}
