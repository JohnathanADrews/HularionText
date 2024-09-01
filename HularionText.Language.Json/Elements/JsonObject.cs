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
    /// Represents a JSON object.
    /// </summary>
    public class JsonObject : JsonElement
    {
        /// <summary>
        /// The type of the element.
        /// </summary>
        public override JsonElementType ElementType { get; protected set; } = JsonElementType.Object;

        /// <summary>
        /// iff true, the case spelling for the member names will not be affected by serializer case modifiers.
        /// </summary>
        public bool MemberNameCaseIsPersistent { get; set; } = false;

        /// <summary>
        /// The values within the object.
        /// </summary>
        public List<JsonElement> Values { get; set; } = new List<JsonElement>();

        /// <summary>
        /// Stores the values into a string-object Dictionary.
        /// </summary>
        /// <returns>The anonymized values.</returns>
        public Dictionary<string, object> MakeAnonymous()
        {
            var result = new Dictionary<string, object>();
            foreach (var item in Values)
            {
                if (result.ContainsKey(item.Name)) { continue; }
                if (item.ElementType == JsonElementType.Object) { result.Add(item.Name, ((JsonObject)item).MakeAnonymous()); }
                if (item.ElementType == JsonElementType.Array) { result.Add(item.Name, ((JsonArray)item).MakeAnonymous()); }
                if (item.ElementType == JsonElementType.String) { result.Add(item.Name, ((JsonString)item).GetValue()); }
                if (item.ElementType == JsonElementType.Number) { result.Add(item.Name, ((JsonNumber)item).GetValue()); }
                if (item.ElementType == JsonElementType.Bool) { result.Add(item.Name, ((JsonBool)item).GetValue()); }
            }
            return result;
        }

        public override void AddNode(JsonElement node)
        {
            Values.Add(node);
        }

        public override string GetStringValue(bool addQuotes = false)
        {
            throw new NotImplementedException("This node does not have a single value.");
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
