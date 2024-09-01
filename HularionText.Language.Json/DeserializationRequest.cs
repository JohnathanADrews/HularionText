#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;
using HularionCore.TypeGraph;
using HularionText.Language.Json.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionText.Language.Json
{
    /// <summary>
    /// A request in the deserialization process.
    /// </summary>
    public class DeserializationRequest
    {
        /// <summary>
        /// The element to deserialize.
        /// </summary>
        public JsonElement Element { get; set; }

        /// <summary>
        /// The destination type of the element content.
        /// </summary>
        public TypeNode Type { get; set; }

        /// <summary>
        /// Provides a TypeNode given a Type.
        /// </summary>
        public IParameterizedProvider<Type, TypeNode> TypeNodeProvider { get; set; }

        /// <summary>
        /// The deserialized value.
        /// </summary>
        internal object DeserializedValue { get; set; }


    }
}
