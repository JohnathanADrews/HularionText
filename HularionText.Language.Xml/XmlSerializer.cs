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

namespace HularionText.Language.Xml
{
    /// <summary>
    /// Serializes XmlDocument and XmlNode to string.
    /// </summary>
    public class XmlSerializer
    {

        private const string tab = "\t";
        private const string newline = "\n";

        /// <summary>
        /// Serializes the XmlDocument to a string.
        /// </summary>
        /// <param name="document">The document to serialize.</param>
        /// <param name="options">Options for serializing an XmlDocument to a string.</param>
        /// <returns>The document serialized as a string.</returns>
        public string Serialize(XmlDocument document, XmlSerializationOptions options)
        {
            return Serialize(options, document.Root.Nodes.ToArray());
        }

        /// <summary>
        /// Serializes the XmlNodes to a string.
        /// </summary>
        /// <param name="node">The node to serialize.</param>
        /// <param name="options">Options for serializing an XmlNodes to a string.</param>
        /// <returns>The node serialized as a string.</returns>
        public string Serialize(XmlNode node, XmlSerializationOptions options)
        {
            return Serialize(options, node);
        }

        /// <summary>
        /// Serializes the XmlNodes to a string.
        /// </summary>
        /// <param name="options">Options for serializing an XmlNodes to a string.</param>
        /// <param name="nodes">The nodes to serialize.</param>
        /// <returns>The nodes serialized as a string.</returns>
        public string Serialize(XmlSerializationOptions options, params XmlNode[] nodes)
        {
            var stringBuilder = new StringBuilder();
            foreach (var node in nodes)
            {
                AddSerialization(options, node, stringBuilder);
            }
            return stringBuilder.ToString();
        }

        private void AddSerialization(XmlSerializationOptions options, XmlNode node, StringBuilder stringBuilder)
        {
            var traverser = new TreeTraverser<XmlNode>();
            int depth = 0;
            var first = true;
            traverser.WeaveExecute(TreeWeaveOrder.FromLeft, node, node => node.Nodes.ToArray(),
                entryAction: state =>
                {
                    if (!first && options.Format == XmlDocumentStringFormat.Indented) 
                    { 
                        if(!state.Subject.IsFreeTextNode || state.Subject.InnerText.Contains(@"\n"))
                        {
                            stringBuilder.Append(newline);
                        }
                    }
                    first = false;
                    if (options.Format == XmlDocumentStringFormat.Indented)
                    {
                        if (!state.Subject.IsFreeTextNode || state.Subject.InnerText.Contains(@"\n"))
                        {
                            for (var i = 0; i < depth; i++) { stringBuilder.Append(tab); }
                        }
                    }
                    if (state.Subject.IsFreeTextNode)
                    {
                        stringBuilder.Append(state.Subject.InnerText);
                        return;
                    }
                    stringBuilder.Append(state.Subject.OpenNodeString);
                    stringBuilder.Append(state.Subject.InnerText);

                    depth++;
                },
                lastAction: state => {
                    if (state.Subject.IsSelfClose && state.Subject.Parent != null && state.Subject.Parent.Nodes.Count == 1)
                    {
                        stringBuilder.Append(newline);
                        depth--;
                        for (var i = 0; i < depth; i++) { stringBuilder.Append(tab); }
                    }
                },
                exitAction: state =>
                {
                    depth--;
                    if (state.Subject.Nodes.Count > 0 && options.Format == XmlDocumentStringFormat.Indented)
                    {
                        if(state.Subject.Nodes.Count != 1 || state.Subject.Nodes.First().InnerText.Contains(@"\n"))
                        {
                            stringBuilder.Append(newline);
                            for (var i = 0; i < depth; i++) { stringBuilder.Append(tab); }
                            depth--;
                        }
                        depth++;
                    }
                    stringBuilder.Append(state.Subject.CloseNodeString);
                });
        }

    }
}
