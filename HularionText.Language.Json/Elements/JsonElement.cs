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

namespace HularionText.Language.Json.Elements
{
    /// <summary>
    /// An element in a JsonDocument.
    /// </summary>
    public abstract class JsonElement
    {
        /// <summary>
        /// The type of element.
        /// </summary>
        public abstract JsonElementType ElementType { get; protected set; }

        /// <summary>
        /// The parent of this element.
        /// </summary>
        public JsonElement Parent { get; set; } = null;

        /// <summary>
        /// The name of this element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of this element prior to name modification.
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// Adds a node to this element.
        /// </summary>
        /// <param name="node"></param>
        public abstract void AddNode(JsonElement node);

        /// <summary>
        /// Gets the string value of this element.
        /// </summary>
        /// <param name="addQuotes">Iff true, quotes will be added to the ends of the string.</param>
        /// <returns>The string value of this element.</returns>
        public abstract string GetStringValue(bool addQuotes = false);

        /// <summary>
        /// Gets the value of the node.
        /// </summary>
        /// <returns>The value of the node.</returns>
        public abstract object GetValue();

        /// <summary>
        /// Gets the nodes for the child elements.
        /// </summary>
        /// <returns></returns>
        public abstract JsonElement[] GetNextNodes();

        protected static JsonElement[] EmptyArray = new JsonElement[] { };


    }
}
