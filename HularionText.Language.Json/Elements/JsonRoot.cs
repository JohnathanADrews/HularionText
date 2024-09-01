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

namespace HularionText.Language.Json.Elements
{
    /// <summary>
    /// A root element for a JSON document. Root is not part of the JSON object.
    /// </summary>
    public class JsonRoot : JsonElement
    {
        /// <summary>
        /// The type of the element.
        /// </summary>
        public override JsonElementType ElementType { get; protected set; } = JsonElementType.Root;

        /// <summary>
        /// The values within the JSON object.
        /// </summary>
        public List<JsonElement> Values { get; set; } = new List<JsonElement>();
        /// <summary>
        /// Converts this document to a structure of dictionaries and lists.
        /// </summary>
        /// <returns>The anonomized object.</returns>
        public object MakeAnonymous()
        {
            object result = new object();
            var rootList = new List<object>();
            result = rootList;
            foreach (var item in Values)
            {
                if (item.ElementType == JsonElementType.Object) { rootList.Add(((JsonObject)item).MakeAnonymous()); }
                if (item.ElementType == JsonElementType.Array) { rootList.Add(((JsonArray)item).MakeAnonymous()); }
            }
            if (rootList.Count == 1) { result = rootList[0]; }
            return result;
        }

        public override void AddNode(JsonElement node)
        {
            Values.Add(node);
        }

        public override string GetStringValue(bool addQuotes = false)
        {
            throw new NotImplementedException("This node does not have a value.");
        }

        public override JsonElement[] GetNextNodes()
        {
            return Values.ToArray();
        }

        public override object GetValue()
        {
            throw new NotImplementedException("This node does not have a value.");
        }
    }

}
