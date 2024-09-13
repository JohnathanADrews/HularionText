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
    public class XmlNode
    {
        /// <summary>
        /// The kind of element this node represents.
        /// </summary>
        public XmlElement Element { get; set; }

        /// <summary>
        /// True iff the node has a self-loing tag. <... /> and not <...>...</...>
        /// </summary>
        public bool IsSelfClose { get; set; }

        /// <summary>
        /// True iff this node represents text without a tag.
        /// </summary>
        public bool IsFreeTextNode { get; set; }

        /// <summary>
        /// The inner text of the node for text nodes like h1, script, or p.
        /// </summary>
        public string InnerText { get; set; } = string.Empty;

        /// <summary>
        /// The parent of this node.
        /// </summary>
        public XmlNode Parent { get; set; }

        /// <summary>
        /// The child nodes of this node.
        /// </summary>
        public List<XmlNode> Nodes { get; set; } = new List<XmlNode>();

        /// <summary>
        /// The attributes of this node.
        /// </summary>
        public List<XmlAttribute> Attributes { get; set; } = new List<XmlAttribute>();

        public IEnumerable<XmlAttribute> GetAttributes(string attribute)
        {
            attribute = attribute.ToLower();
            return Attributes.Where(x => x.Name.ToLower() == attribute).ToList();
        }

        public string GetAttributeValue(string attribute, bool trimResult = false, bool returnEmptyStringIfNull = false)
        {
            var nodeAttribute = GetAttributes(attribute).FirstOrDefault();
            if (nodeAttribute == null || nodeAttribute.Value == null) 
            {
                if (returnEmptyStringIfNull) { return string.Empty; }
                return null; 
            }
            if (trimResult) { return nodeAttribute.Value.Trim(); }
            return nodeAttribute.Value;
        }

        public bool HasAttribute(string attribute)
        {
            return GetAttributes(attribute).Count() > 0;
        }


        private const string singleQuote = "'";
        private const string doubleQuote = "\"";
        private const string noQuote = "";
        private const string openAngle = "<";
        private const string closeAngle = ">";
        private const string tagClose = "</";
        private const string tagSelfClose = "/>";
        private const string equals = "=";
        private const string space = " ";

        public string OpenNodeString
        {
            get
            {
                if (Element == null) { return string.Empty; }
                var builder = new StringBuilder();
                builder.Append(openAngle);
                if(Element.IsUnknown)
                {
                    //builder.Append(ElementText);
                    builder.Append(Element.Text);
                }
                else
                {
                    builder.Append(Element.Text);
                }
                string quote = singleQuote;
                foreach (var attributte in Attributes)
                {
                    quote = noQuote;
                    if (attributte.QuoteType == AttributeQuoteType.Single) { quote = singleQuote; }
                    else if (attributte.QuoteType == AttributeQuoteType.Double) { quote = doubleQuote; }
                    builder.Append(space);
                    builder.Append(attributte.Name);
                    if (attributte.Value != null)
                    {
                        builder.Append(equals);
                        builder.Append(quote);
                        builder.Append(attributte.Value);
                        builder.Append(quote);
                    }
                }
                if (IsSelfClose) { builder.Append(tagSelfClose); }
                else { builder.Append(closeAngle); }
                return builder.ToString();
            }
        }

        public string CloseNodeString
        {
            get
            {
                if (Element == null) { return string.Empty; }
                if (IsSelfClose) { return string.Empty; }
                if (Element.IsUnknown)
                {
                    //return String.Format("{0}{1}{2}", tagClose, ElementText, closeAngle);
                    return String.Format("{0}{1}{2}", tagClose, Element.Text, closeAngle);
                }
                else
                {
                    return String.Format("{0}{1}{2}", tagClose, Element.Text, closeAngle);
                }
            }
        }

        public IEnumerable<XmlAttribute> GetAttributesHavingPrefix(string prefix)
        {
            var result = new List<XmlAttribute>();
            foreach (var attribute in Attributes)
            {
                if (attribute.Name != null && attribute.Name.StartsWith(prefix)) { result.Add(attribute); }
            }
            return result;
        }

        public IEnumerable<XmlNode> GetNodesOfElementType(string type)
        {
            var nodes = new List<XmlNode>();
            var traverser = new TreeTraverser<XmlNode>();
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, this, node => node.Nodes.ToArray(), true);
            foreach (var node in plan)
            {
                if (node.Element != null && node.Element.Text == type) { nodes.Add(node); }
            }
            return nodes;
        }

        public void AddNode(XmlNode node)
        {
            Nodes.Add(node);
        }

        public void RemoveNodes(params XmlNode[] nodes)
        {
            foreach (var node in nodes)
            {
                node.Parent.Nodes.Remove(node);
            }
        }


        public string ToDocumentString(XmlSerializationOptions options)
        {
            var serializer = new XmlSerializer();
            return serializer.Serialize(this, options);
        }

        /// <summary>
        /// If this node is the parent of a single free text node, then this will return the content of that node and null otherwise.
        /// </summary>
        /// <returns>If this node is the parent of a single free text node, then this will return the content of that node and null otherwise.</returns>
        public string GetInnerFreeText()
        {
            if(Nodes != null && Nodes.Count() == 1 && Nodes.First().IsFreeTextNode)
            {
                return Nodes.First().InnerText;
            }
            return null;
        }

        /// <summary>
        /// Gets the nodes InnerText if !String.IsNullOrWhiteSpace. Otherwise, returns GetInnerFreeText()
        /// </summary>
        /// <returns>Gets the nodes InnerText if !String.IsNullOrWhiteSpace. Otherwise, returns GetInnerFreeText()</returns>
        public string GetCoalesceText()
        {
            if (!String.IsNullOrWhiteSpace(InnerText))
            {
                return InnerText;
            }
            return GetInnerFreeText();
        }

        /// <summary>
        /// Gets the first available attribute with matching name or null if there are no matches.
        /// </summary>
        /// <param name="attributeNames">The names of the attributes.</param>
        /// <param name="includeNull">iff true, return the attrubute even if the value is null;</param>
        /// <param name="includeIfEmpty">iff true, return the attrubute even if the value is String.IsNullOrWhiteSpace()</param>
        /// <returns>The first matching attribute or null if none match.</returns>
        public XmlAttribute? GetFirstAvailableAttribute(string[] attributeNames, bool includeNull = false, bool includeIfEmpty = false)
        {
            var attributeMap = Attributes.Where(x => !String.IsNullOrWhiteSpace(x.Name)).ToDictionary(x => x.Name.Trim().ToLower(), x => x);
            attributeNames = attributeNames.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x=>x.Trim().ToLower()).ToArray();
            for (var i = 0; i < attributeNames.Length; i++)
            {
                if (attributeMap.ContainsKey(attributeNames[i]))
                {
                    var attribute = attributeMap[attributeNames[i]];
                    if(attribute.Value == null && !includeNull)
                    {
                        continue;
                    }
                    if(String.IsNullOrWhiteSpace(attribute.Value) && !includeIfEmpty)
                    {
                        continue;
                    }
                    return attribute;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the first attribute value where !String.IsNullOrWhiteSpace().
        /// </summary>
        /// <param name="attributeNames">The names of the attributes</param>
        /// <returns>The first attribute value where !String.IsNullOrWhiteSpace().</returns>
        public string GetFirstAvailableAttributeValue(params string[] attributeNames)
        {
            var attribute = GetFirstAvailableAttribute(attributeNames);
            if(attribute == null)
            {
                return null;
            }
            return attribute.Value;
        }

        /// <summary>
        /// Gets the value of the first available indicated attributes or the inner text GetCoalesceText()
        /// </summary>
        /// <param name="attributeNames">The names of the attributes to check.</param>
        /// <param name="attributeFirst">iff true (default), gets the attribute name first if a match is found. Otherwise, it will get the inner text first.</param>
        /// <returns>The value of the first matching attribute with a value or GetCoalesceText() if no attribute was found.</returns>
        public string CoalesceAttributeValueOrNodeText(string[] attributeNames, bool attributeFirst = true)
        {
            if (attributeFirst)
            {
                var attribute = GetFirstAvailableAttribute(attributeNames);
                if (attribute != null)
                {
                    return attribute.Value;
                }
                return GetCoalesceText();
            }
            var result = GetCoalesceText();
            if (!String.IsNullOrWhiteSpace(result))
            {
                return result;
            }
            else
            {
                var attribute = GetFirstAvailableAttribute(attributeNames);
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the value of the first available indicated attributes or the inner text GetCoalesceText()
        /// </summary>
        /// <param name="attributeName">The name of the attributes to check.</param>
        /// <param name="attributeFirst">iff true (default), gets the attribute name first if a match is found. Otherwise, it will get the inner text first.</param>
        /// <returns>The value of the first matching attribute with a value or GetCoalesceText() if no attribute was found.</returns>
        public string CoalesceAttributeValueOrNodeText(string attributeName, bool attributeFirst = true)
        {
            return CoalesceAttributeValueOrNodeText(new string[] { attributeName }, attributeFirst: attributeFirst);
        }

        /// <summary>
        /// Gets all the ancestors of this node, with its parent at index 0.
        /// </summary>
        /// <returns>The ancestors of this node, with its parent at index 0.</returns>
        public XmlNode[] GetAncestors()
        {
            var result = new List<XmlNode>();
            var parent = Parent;
            while(parent != null)
            {
                result.Add(parent);
                parent = parent.Parent;
            }
            return result.ToArray();
        }


        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append("<");
            if (Element == null)
            {
                result.Append("unknown");
            }
            else
            {
                result.Append(Element.Text);
            }
            foreach(var attribute in Attributes)
            {
                result.Append(" ");
                result.Append(attribute.ToString());
            }
            result.Append(" ");
            if (IsSelfClose)
            {
                result.Append("/>");
            }
            else
            {
                result.Append(String.Format("> ... {0} Nodes ... </{1}>", Nodes.Count, Element == null ? "unknown" : Element.Text));
            }
            return result.ToString();
        }
    }
}
