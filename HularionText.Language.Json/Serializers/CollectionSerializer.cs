#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionText.Language.Json.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HularionText.Language.Json.Serializers
{
    /// <summary>
    /// Contains the serialization details for collection types.
    /// </summary>
    public class CollectionSerializer
    {

        /// <summary>
        /// Creates the serializer.
        /// </summary>
        /// <returns>The serializer.</returns>
        public TypeSerializer MakeSerializer()
        {
            var collectionSerializer = new TypeSerializer();
            var listType = typeof(List<>);
            var hashSetType = typeof(HashSet<>);
            collectionSerializer.Serialize = detail => new JsonArray();
            collectionSerializer.Deserialize = detail =>
            {
                var values = new List<object>();
                //If the member is null, the value is JsonElementType.Unknown
                if (detail.Element.ElementType == JsonElementType.Array) 
                {
                    values = ((JsonArray)detail.Element).Values.Select(x => detail.ValueMap[x]).ToList();
                }
                Type type = null;
                if (detail.TypedValue.Type.IsArray) { type = detail.TypedValue.Type.GetElementType(); }
                else { type = detail.TypedValue.Type.GetGenericArguments().First(); }
                var array = Array.CreateInstance(type, values.Count);
                for (var i = 0; i < values.Count; i++) { array.SetValue(values[i], i); }

                if (detail.TypedValue.Type.IsArray) { return array; }
                if (detail.TypedValue.Type.GetGenericTypeDefinition() == hashSetType) { return Activator.CreateInstance(hashSetType.MakeGenericType(type), new object[] { array }); }
                return Activator.CreateInstance(listType.MakeGenericType(type), new object[] { array });
            };
            collectionSerializer.GetNextValues = typedValue =>
            {
                var result = new List<TypedValue>();
                if (typedValue.Value == null) { return result; }
                var type = typedValue.Value.GetType();
                Type nextType = null;
                if (type.IsGenericType) { nextType = type.GetGenericArguments().First(); }
                if (type.IsArray) { nextType = type.GetElementType(); }

                var emptyArray = new object[] { };
                var enumerator = type.GetMethod("GetEnumerator").Invoke(typedValue.Value, emptyArray);
                var moveNext = enumerator.GetType().GetMethod("MoveNext");
                var current = enumerator.GetType().GetProperty("Current");
                while ((bool)moveNext.Invoke(enumerator, emptyArray))
                {
                    result.Add(new TypedValue() { Type = nextType, Value = current.GetValue(enumerator) });
                }
                return result;
            };
            return collectionSerializer;
        }

        /// <summary>
        /// Sets the serializr for the related types.
        /// </summary>
        /// <param name="serializer">The serializer in which to set the type serializers.</param>
        public void SetSerializerTypes(JsonSerializer serializer)
        {
            var typeSerializer = MakeSerializer();
            serializer.SetTypeSerializer(typeof(IEnumerable<>), typeSerializer);
            serializer.SetTypeSerializer(typeof(IList<>), typeSerializer);
            serializer.SetTypeSerializer(typeof(List<>), typeSerializer);
            serializer.SetTypeSerializer(typeof(HashSet<>), typeSerializer);
            serializer.SetTypeSerializer(typeof(Array), typeSerializer);

        }
    }
}
