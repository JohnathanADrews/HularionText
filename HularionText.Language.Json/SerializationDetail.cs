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
    /// Contains the details for serializing and deserializing values.
    /// </summary>
    public class SerializationDetail
    {
        /// <summary>
        /// Information related to the value.
        /// </summary>
        public TypedValue TypedValue { get; set; }

        /// <summary>
        /// The element to deserialize.
        /// </summary>
        public JsonElement Element { get; set; }

        /// <summary>
        /// Provides the TypeNode for the given type.
        /// </summary>
        /// <remarks>Use the TypeNode of a type to get its members and get/set values.</remarks>
        public IParameterizedProvider<Type, TypeNode> TypeNodeProvider { get; set; }

        /// <summary>
        /// The value map used in the deserialization process.
        /// </summary>
        public Dictionary<JsonElement, object> ValueMap { get; set; }

        /// <summary>
        /// The spacing used in the serialization.
        /// </summary>
        public JsonSerializationSpacing Spacing { get; set; }

    }
}
