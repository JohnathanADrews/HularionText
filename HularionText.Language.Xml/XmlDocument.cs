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

namespace HularionText.Language.Xml
{
    /// <summary>
    /// A document that describes an XML page or fragment.
    /// </summary>
    public class XmlDocument
    {
        /// <summary>
        /// The root node of the documemt.
        /// </summary>
        public XmlNode Root { get; set; } = new XmlNode();

        /// <summary>
        /// The attributes in the document with the corresponding nodes.
        /// </summary>
        public IDictionary<string, List<XmlAttribute>> Attributes { get; private set; } = new Dictionary<string, List<XmlAttribute>>();


        /// <summary>
        /// Adds an attribute to the current top node.
        /// </summary>
        public void AddAttribute(XmlAttribute attribute, XmlNode node)
        {
            var lower = attribute.Name.ToLower();
            if (!Attributes.ContainsKey(lower)) { Attributes.Add(lower, new List<XmlAttribute>()); }
            Attributes[lower].Add(attribute);
        }

        /// <summary>
        /// Adds the attributes to the current top node.
        /// </summary>
        public void AddAttributes(params XmlAttribute[] attributes)
        {
            foreach (var attribute in attributes) { AddAttribute(attribute, attribute.Node); }
        }

        /// <summary>
        /// Gets all of the having the attribute.
        /// </summary>
        /// <param name="attribute">The name of the attribute.</param>
        /// <returns>All of the having the attribute.</returns>
        public IEnumerable<XmlNode> GetAttibuteNodes(string attribute)
        {
            attribute = attribute.ToLower();
            if (!Attributes.ContainsKey(attribute)) { return new List<XmlNode>(); }
            return Attributes[attribute].Select(x => x.Node).ToList();
        }

        /// <summary>
        /// Gets all of the having the attributes.
        /// </summary>
        /// <param name="selected">The names of the attributes.</param>
        /// <returns>All of the having all of the attributes.</returns>
        public IEnumerable<XmlNode> GetAttibuteNodes(params string[] selected)
        {
            selected = selected.Select(x => x.ToLower()).ToArray();
            var nodes = new List<XmlNode>();
            foreach (var attribute in selected)
            {
                if (!this.Attributes.ContainsKey(attribute)) { return new List<XmlNode>(); }
                if (nodes.Count == 0) { nodes = Attributes[attribute].Select(x => x.Node).ToList(); continue; }
                nodes = Attributes[attribute].Select(x => x.Node).ToList().Intersect(nodes).ToList();
            }
            return nodes;
        }

        public void ProcessNodesHavingAttribute(ProcessMode mode, string attribute, Action<XmlNode> action)
        {
            var nodes = GetAttibuteNodes(attribute);
            foreach (var node in nodes)
            {
                action(node);
                if (mode == ProcessMode.FirstNode) { return; }
            }
        }

        public void ProcessAttributesOfNodeHavingAttribute(ProcessMode mode, string attribute, IDictionary<string, Action<XmlNode, XmlAttribute>> actions, Action<XmlNode> findAction = null)
        {
            if (findAction == null) { findAction = n => { }; }
            var nodes = GetAttibuteNodes(attribute);
            foreach (var node in nodes)
            {
                findAction(node);
                foreach (var na in node.Attributes)
                {
                    if (actions.ContainsKey(na.Name)) { actions[na.Name](node, na); }
                }
                if (mode == ProcessMode.FirstNode) { return; }
            }
        }

        public IEnumerable<XmlNode> GetNodesOfElementType(string type)
        {
            return Root.GetNodesOfElementType(type);
        }

        public void RemoveNodes(params XmlNode[] nodes)
        {
            Root.RemoveNodes(nodes);
        }

        public string ToDocumentString(XmlSerializationOptions options)
        {
            var serializer = new XmlSerializer();
            return serializer.Serialize(this, options);
        }

    }




    public enum ProcessMode
    {
        FirstNode,
        EveryNode
    }

    public enum XmlDocumentStringFormat
    {
        Indented,
        Minimal
    }
}
