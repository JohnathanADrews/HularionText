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

namespace HularionText.Language.Json.Serializers
{
    /// <summary>
    /// Contains the serialization details for Dictionary types.
    /// </summary>
    public class DictionarySerializer
    {
        /// <summary>
        /// Creates the serializer.
        /// </summary>
        /// <returns>The serializer.</returns>
        public TypeSerializer MakeSerializer()
        {
            var stringType = typeof(string);
            var emptyArray = new object[] { };

            var dictionarySerializer = new TypeSerializer();
            dictionarySerializer.Serialize = detail =>
            {
                JsonElement element;
                if (detail.TypedValue.Type.GenericTypeArguments[0] == stringType) { element = new JsonObject() { MemberNameCaseIsPersistent = true }; }
                else { element = new JsonArray(); }
                return element;
            };
            dictionarySerializer.Deserialize = detail =>
            {
                var result = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(detail.TypedValue.Type.GenericTypeArguments));

                if (detail.Element.ElementType == JsonElementType.Array)
                {
                    var values = ((JsonArray)detail.Element).Values;
                    if (values.Count == 0) 
                    { 
                        return result; 
                    }
                    Type valueType = detail.ValueMap[values.First()].GetType();
                    var keyProperty = valueType.GetProperty("Key");
                    var valueProperty = valueType.GetProperty("Value");
                    var addMethod = detail.TypedValue.Type.GetMethod("Add");
                    foreach (var element in values)
                    {
                        var keyValuePair = detail.ValueMap[element];
                        addMethod.Invoke(result, new object[] { keyProperty.GetValue(keyValuePair), valueProperty.GetValue(keyValuePair) });
                    }
                }
                if (detail.Element.ElementType == JsonElementType.Object)
                {
                    var values = ((JsonObject)detail.Element).Values;
                    var addMethod = detail.TypedValue.Type.GetMethod("Add");
                    foreach (var element in values)
                    {
                        addMethod.Invoke(result, new object[] { element.Name, detail.ValueMap[element] });
                    }
                }
                return result;
            };
            dictionarySerializer.GetNextValues = typedValue =>
            {
                var result = new List<TypedValue>();
                if(typedValue.Value == null) { return result; }
                var type = typedValue.Value.GetType();

                Type nextType = typeof(KeyValuePair<,>).MakeGenericType(type.GetGenericArguments());
                var isObject = (type.GenericTypeArguments[0] == stringType);

                var enumerator = type.GetMethod("GetEnumerator").Invoke(typedValue.Value, emptyArray);
                var moveNext = enumerator.GetType().GetMethod("MoveNext");
                var current = enumerator.GetType().GetProperty("Current");
                var keyProperty = nextType.GetProperty("Key");
                var valueProperty = nextType.GetProperty("Value");
                while ((bool)moveNext.Invoke(enumerator, emptyArray))
                {
                    var nextValue = new TypedValue() { Type = nextType, Value = current.GetValue(enumerator) };
                    if (isObject)
                    {
                        nextValue.Name = (string)keyProperty.GetValue(nextValue.Value);
                        nextValue.Value = valueProperty.GetValue(nextValue.Value);
                        nextValue.Type = nextValue.Value == null ? null : nextValue.Value.GetType();
                    }
                    result.Add(nextValue);
                }
                return result;
            };

            dictionarySerializer.GetNextDeserializationRequests = request =>
            {
                var next = new List<DeserializationRequest>();
                var elements = request.Element.GetNextNodes();
                if (request.Element.ElementType == JsonElementType.Object)
                {
                    foreach (var element in elements)
                    {
                        var nextNode = new DeserializationRequest() 
                        { 
                            Element = element, 
                            Type = request.TypeNodeProvider.Provide(request.Type.Type.GetGenericArguments()[1])
                        };
                        next.Add(nextNode);
                    }
                }
                if (request.Element.ElementType == JsonElementType.Array)
                {
                    foreach (var element in elements)
                    {
                        var nextNode = new DeserializationRequest() 
                        { 
                            Element = element, 
                            Type = request.TypeNodeProvider.Provide(typeof(KeyValuePair<,>).MakeGenericType(request.Type.Type.GetGenericArguments())) 
                        };
                        next.Add(nextNode);
                    }
                }
                return next;
            };

            return dictionarySerializer;
        }

        /// <summary>
        /// Sets the serializr for the related types.
        /// </summary>
        /// <param name="serializer">The serializer in which to set the type serializers.</param>
        public void SetSerializerTypes(JsonSerializer serializer)
        {
            var typeSerializer = MakeSerializer();
            serializer.SetTypeSerializer(typeof(IDictionary<,>), typeSerializer);
            serializer.SetTypeSerializer(typeof(Dictionary<,>), typeSerializer);
        }

    }
}
