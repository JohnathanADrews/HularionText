#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Topology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionText.Language.Html
{
    /// <summary>
    /// Serializes HtmlDocument and HtmlNode to string.
    /// </summary>
    public class HtmlSerializer
    {

        private const string tab = "\t";
        private const string newline = "\n";

        /// <summary>
        /// Serializes the HtmlDocument to a string.
        /// </summary>
        /// <param name="document">The document to serialize.</param>
        /// <param name="options">Options for serializing an HtmlDocument to a string.</param>
        /// <returns>The document serialized as a string.</returns>
        public string Serialize(HtmlDocument document, HtmlSerializationOptions options)
        {
            return Serialize(options, document.Root.Nodes.ToArray());
        }

        /// <summary>
        /// Serializes the HtmlNodes to a string.
        /// </summary>
        /// <param name="node">The node to serialize.</param>
        /// <param name="options">Options for serializing an HtmlNodes to a string.</param>
        /// <returns>The node serialized as a string.</returns>
        public string Serialize(HtmlNode node, HtmlSerializationOptions options)
        {
            return Serialize(options, node);
        }

        /// <summary>
        /// Serializes the HtmlNodes to a string.
        /// </summary>
        /// <param name="options">Options for serializing an HtmlNodes to a string.</param>
        /// <param name="nodes">The nodes to serialize.</param>
        /// <returns>The nodes serialized as a string.</returns>
        public string Serialize(HtmlSerializationOptions options, params HtmlNode[] nodes)
        {
            var stringBuilder = new StringBuilder();
            foreach (var node in nodes)
            {
                AddSerialization(options, node, stringBuilder);
            }
            return stringBuilder.ToString();
        }

        private void AddSerialization(HtmlSerializationOptions options, HtmlNode node, StringBuilder stringBuilder)
        {
            var traverser = new TreeTraverser<HtmlNode>();
            int depth = 0;
            var first = true;
            traverser.WeaveExecute(TreeWeaveOrder.FromLeft, node, node => node.Nodes.ToArray(),
                entryAction: state =>
                {
                    if (!first && options.Format == HtmlDocumentStringFormat.Indented) { stringBuilder.Append(String.Format(newline)); }
                    first = false;
                    if (options.Format == HtmlDocumentStringFormat.Indented)
                    {
                        for (var i = 0; i < depth; i++) { stringBuilder.Append(tab); }
                    }
                    if (state.Subject.IsFreeTextNode)
                    {
                        stringBuilder.Append(state.Subject.InnerText);
                        return;
                    }
                    stringBuilder.Append(state.Subject.OpenNodeString);
                    if (state.Subject.Element.HasInnerLanguage)
                    {
                        if (!String.IsNullOrWhiteSpace(state.Subject.EmbeddedLanguageContent))
                        {
                            stringBuilder.Append(String.Format(newline));
                            stringBuilder.Append(state.Subject.EmbeddedLanguageContent);
                        }
                    }
                    else
                    {
                        stringBuilder.Append(state.Subject.InnerText);
                    }
                    depth++;
                },
                upAction: state => { },
                exitAction: state =>
                {
                    depth--;
                    if (state.Subject.Nodes.Count > 0 && options.Format == HtmlDocumentStringFormat.Indented)
                    {
                        stringBuilder.Append(String.Format(newline));
                        for (var i = 0; i < depth; i++) { stringBuilder.Append(tab); }
                    }
                    stringBuilder.Append(state.Subject.CloseNodeString);
                });
        }

    }
}
