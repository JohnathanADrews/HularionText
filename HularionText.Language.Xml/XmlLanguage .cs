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

namespace HularionText.Language.Xml
{
    /// <summary>
    /// A language parser for XML that can convert XML string into a XmlDocument.
    /// </summary>
    public class XmlLanguage
    {

        public TextSequenceTree SymbolTree { get; set; } = new TextSequenceTree() { Name = "Xml Symbol Tree" };

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

        Table<SequenceTreeTextNode, XmlOperationMode> actionTable = new Table<SequenceTreeTextNode, XmlOperationMode>();
        Action<XmlContext> intermediateAction = new Action<XmlContext>(context => { });

        XmlOperationMode startMode = new XmlOperationMode() { Name = "Start" };
        XmlOperationMode tagOpenMode = new XmlOperationMode() { Name = "Tag Open" };
        XmlOperationMode nodeEndMode = new XmlOperationMode() { Name = "End Node" };
        XmlOperationMode tagContentMode = new XmlOperationMode() { Name = "Tag Content" };
        XmlOperationMode nodeContentMode = new XmlOperationMode() { Name = "Node Content" };
        XmlOperationMode nodeTextContentMode = new XmlOperationMode() { Name = "Node Text Content" };
        XmlOperationMode nodeClosedMode = new XmlOperationMode() { Name = "Node Closed" };
        XmlOperationMode attributeValueMode = new XmlOperationMode() { Name = "Attribute Value Mode" };
        XmlOperationMode attributeSingleQuoteMode = new XmlOperationMode() { Name = "Attribute Single Quote" };
        XmlOperationMode attributeDoubleQuoteMode = new XmlOperationMode() { Name = "Attribute Double Quote" };
        XmlOperationMode commentMode = new XmlOperationMode() { Name = "Comment" };

        private char[] whitespace = new char[] { ' ', '\t', '\r', '\n', (char)65279 };

        public XmlLanguage()
        {
            intermediateAction = new Action<XmlContext>(context =>
            {
                if (context.Mode == startMode || context.Mode == nodeContentMode)
                {
                    var node = new XmlNode() { IsFreeTextNode = true, Parent = context.Node };
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
                        context.Attribute = new XmlAttribute() { Name = text, Node = context.Node, HasEquals = false, QuoteType = AttributeQuoteType.NoQuote };
                        context.Node.Attributes.Add(context.Attribute);
                        context.Document.AddAttributes(context.Attribute);
                    }
                    return;
                }
            });

            #region NewNode
            AddSymbolModes(openNodeBegin, new Action<XmlContext>(context =>
            {
                context.ModeStack.Push(tagOpenMode);
                var node = new XmlNode() { Parent = context.Node };
                context.Node.AddNode(node);
                context.Node = node;
                context.ValueStartIndex = context.Match.NextIndex;
            }), startMode, nodeContentMode, nodeClosedMode);

            AddSymbolModes(closeTag, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                //if (context.Node.Element.IsTextContainer)
                //{
                //    context.ModeStack.Push(nodeTextContentMode);
                //}
                //else
                //{
                //    context.ModeStack.Push(nodeContentMode);
                //}
                context.ModeStack.Push(nodeContentMode);

                context.ValueStartIndex = context.Match.NextIndex;
            }), tagOpenMode, tagContentMode);

            AddSymbolModes(selfCloseNode, new Action<XmlContext>(context =>
            {
                context.Node.IsSelfClose = true;
                context.ModeStack.Pop();
                context.Node = context.Node.Parent;
                context.ValueStartIndex = context.Match.NextIndex;
            }), tagOpenMode, tagContentMode);

            AddModeSymbols(tagOpenMode, new Action<XmlContext>(context =>
            {
                context.ValueStartIndex = context.Match.NextIndex;
                context.ModeStack.Pop();
                context.ModeStack.Push(tagContentMode);
            }), newline, carriageReturn, space, tab);

            #endregion

            #region CloseNode
            AddSymbolMode(nodeContentMode, openNodeEnd, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Push(nodeEndMode);
                context.ValueStartIndex = context.Match.NextIndex;
            }));
            AddModeSymbols(nodeTextContentMode, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Push(nodeEndMode);
                context.Node.InnerText = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.ValueStartIndex = context.Match.NextIndex;
            }), openNodeEnd);
            AddSymbolMode(nodeEndMode, closeTag, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Node = context.Node.Parent;
                context.ValueStartIndex = context.Match.NextIndex;
            }));
            #endregion

            #region Attributes

            AddModeSymbols(tagContentMode, new Action<XmlContext>(context =>
            {
                context.ModeStack.Push(attributeValueMode);
                context.ValueStartIndex = context.Match.NextIndex;
                context.Attribute.HasEquals = true;
            }), operatorEquals);

            AddModeSymbols(attributeValueMode, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Push(attributeDoubleQuoteMode);
                context.ValueStartIndex = context.Match.NextIndex;
                context.Attribute.QuoteType = AttributeQuoteType.Double;
            }), doubleQuote);

            AddModeSymbols(attributeValueMode, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Push(attributeSingleQuoteMode);
                context.ValueStartIndex = context.Match.NextIndex;
                context.Attribute.QuoteType = AttributeQuoteType.Single;
            }), singleQuote);

            AddModeSymbols(attributeValueMode, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Attribute.Value = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.Attribute.QuoteType = AttributeQuoteType.NoQuote;
                context.Attribute.HasEquals = true;
                context.ValueStartIndex = context.Match.NextIndex;
            }), space, tab, newline, carriageReturn);

            AddModeSymbols(attributeValueMode, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ModeStack.Pop();
                context.ModeStack.Push(nodeContentMode);
                context.Attribute.Value = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.Attribute.QuoteType = AttributeQuoteType.NoQuote;
                context.Attribute.HasEquals = true;
                context.ValueStartIndex = context.Match.NextIndex;
            }), closeTag);

            AddModeSymbols(attributeDoubleQuoteMode, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Attribute.Value = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.Attribute.QuoteType = AttributeQuoteType.Double;
            }), doubleQuote);

            AddModeSymbols(attributeSingleQuoteMode, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.Attribute.Value = context.Text.Substring(context.ValueStartIndex, context.Match.StartIndex - context.ValueStartIndex);
                context.Attribute.QuoteType = AttributeQuoteType.Single;
            }), singleQuote);

            AddModeSymbols(tagContentMode, new Action<XmlContext>(context =>
            {
                context.ValueStartIndex = context.Match.NextIndex;
            }), space, tab, newline, carriageReturn);


            #endregion

            #region Comments

            AddSymbolModes(startComment, new Action<XmlContext>(context =>
            {
                context.ModeStack.Push(commentMode);
            }), startMode, nodeContentMode);

            AddSymbolModes(endComment, new Action<XmlContext>(context =>
            {
                context.ModeStack.Pop();
                context.ValueStartIndex = context.Match.NextIndex;
            }), commentMode);

            #endregion
        }

        /// <summary>
        /// Parses the xml text, creating a XmlDocument.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>A XmlDocument.</returns>
        public XmlDocument Parse(string text)
        {
            var context = new XmlContext() { TextTree = SymbolTree, Text = text, Document = new XmlDocument(), Node = new XmlNode() };
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
                    else if (actionTable.ContainsValue(match.Match, (XmlOperationMode)context.Mode))
                    {
                        var action = actionTable.GetValue<Action<XmlContext>>(match.Match, (XmlOperationMode)context.Mode);
                        action(context);
                    }
                    index = matches[i].NextIndex;
                }
            }

            return context.Document;
        }



        private void AddModeSymbols(XmlOperationMode mode, Action<XmlContext> action, params TextSymbol[] symbols)
        {
            foreach (var symbol in symbols)
            {
                AddSymbolMode(mode, symbol, action);
            }
        }

        private void AddSymbolModes(TextSymbol symbol, Action<XmlContext> action, params XmlOperationMode[] modes)
        {
            foreach (var mode in modes)
            {
                AddSymbolMode(mode, symbol, action);
            }
        }

        private void AddSymbolMode(XmlOperationMode mode, TextSymbol symbol, Action<XmlContext> action)
        {
            var sequenceNode = SymbolTree.AddSymbol(symbol, mode);
            actionTable.AddColumn(sequenceNode);
            actionTable.AddRow(mode);
            actionTable.SetValue(sequenceNode, mode, action);
        }



    }
}
