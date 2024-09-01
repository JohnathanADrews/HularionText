#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.TypeGraph;
using HularionText.Language.Json.Elements;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HularionText.Language.Json
{
    /// <summary>
    /// Contains the serialization details for a given type.
    /// </summary>
    public class TypeSerializer
    {
        /// <summary>
        /// The type that is set for the deserializer. 
        /// </summary>
        public Type Type { get; internal set; }

        /// <summary>
        /// This function must serialize an object to a JsonElement using the provided detail object.
        /// </summary>
        public Func<SerializationDetail, JsonElement> Serialize { get; set; }

        /// <summary>
        /// This function must deserialize the JsonElement in the detail object to a typed object.
        /// </summary>
        public Func<SerializationDetail, object> Deserialize { get; set; }

        /// <summary>
        /// When serializing to a JsonArray or JsonObject, this function must provide the next values to serialize.
        /// </summary>
        public Func<TypedValue, IEnumerable<TypedValue>> GetNextValues { get; set; } = value => new TypedValue[] { };

        /// <summary>
        /// If not null, this will provide the next elements in the serialization tree. Use if the type is not a normal value, object, or array.
        /// </summary>
        public Func<DeserializationRequest, IEnumerable<DeserializationRequest>> GetNextDeserializationRequests { get; set; }

        /// <summary>
        /// Deserializers for particular properties of Type.
        /// </summary>
        public IDictionary<PropertyInfo, Func<SerializationDetail, object>> PropertyDeserializers = new Dictionary<PropertyInfo, Func<SerializationDetail, object>>();

        /// <summary>
        /// Deserializers for particular fields of Type.
        /// </summary>
        public IDictionary<PropertyInfo, Func<SerializationDetail, object>> FieldDeserializers = new Dictionary<PropertyInfo, Func<SerializationDetail, object>>();


    }
}
