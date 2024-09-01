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
    /// Represents a JSON boolean vlaue.
    /// </summary>
    public class JsonBool : JsonElement
    {
        /// <summary>
        /// The type of the element.
        /// </summary>
        public override JsonElementType ElementType { get; protected set; } = JsonElementType.Bool;

        /// <summary>
        /// The value of the element.
        /// </summary>
        public bool Value { get; set; }


        public override void AddNode(JsonElement node)
        {
            throw new NotImplementedException("This node cannot add nodes.");
        }

        public override string GetStringValue(bool addQuotes = false)
        {
            if (addQuotes) { return String.Format("\"{0}\"", Value ? "true" : "false"); }
            return Value ? "true" : "false";
        }

        public override object GetValue()
        {
            return Value;
        }

        public override JsonElement[] GetNextNodes()
        {
            return EmptyArray;
        }
    }
}
