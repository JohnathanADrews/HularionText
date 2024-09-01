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
    /// Represents a JSON number.
    /// </summary>
    public class JsonNumber : JsonElement
    {
        /// <summary>
        /// The type of the element.
        /// </summary>
        public override JsonElementType ElementType { get; protected set; } = JsonElementType.Number;

        /// <summary>
        /// The value of the number as read from a JSON string.
        /// </summary>
        public string Value { get; set; }

        private static string longCharactersMin = long.MinValue.ToString();
        private static string longCharactersMax = long.MaxValue.ToString();


        public override void AddNode(JsonElement node)
        {
            throw new NotImplementedException("This node cannot add nodes.");
        }

        public override string GetStringValue(bool addQuotes = false)
        {
            if (addQuotes) { return String.Format("\"{0}\"", Value); }
            return Value;
        }

        /// <summary>
        /// Parses the Value and returns a number type.
        /// </summary>
        /// <returns>The parsed number value.</returns>
        public override object GetValue()
        {
            if (Value.Contains("E") || Value.Contains("e"))
            {
                double r = 0;
                double.TryParse(Value, out r);
                return r;
            }
            else if (Value.Contains(".")
                || (Value.StartsWith("-") && Value.Length > longCharactersMin.Length)
                || Value.Length > longCharactersMax.Length)
            {
                decimal r = 0;
                decimal.TryParse(Value, out r);
                return r;
            }
            else
            {
                long r = 0;
                long.TryParse(Value, out r);
                return r;
            }
        }

        public override JsonElement[] GetNextNodes()
        {
            return EmptyArray;
        }

    }
}
