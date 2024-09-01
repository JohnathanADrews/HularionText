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
    /// Contains the serialization details for the KeyValuePair type.
    /// </summary>
    public class KeyValuePairSerializer
    {
        /// <summary>
        /// Creates the serializer.
        /// </summary>
        /// <returns>The serializer.</returns>
        public TypeSerializer MakeSerializer()
        {

            var keyValuePairSerializer = new TypeSerializer();
            keyValuePairSerializer.Serialize = detail => new JsonObject();
            keyValuePairSerializer.Deserialize = detail =>
            {
                var jsonObject = (JsonObject)detail.Element;
                var keyValue = detail.ValueMap[jsonObject.Values.Where(x => x.Name == "Key").First()];
                var valueValue = detail.ValueMap[jsonObject.Values.Where(x => x.Name == "Value").First()];
                var value = Activator.CreateInstance(detail.TypedValue.Type, new object[] { keyValue, valueValue });
                return value;
            };
            keyValuePairSerializer.GetNextValues = typedValue =>
            {
                var keyProperty = typedValue.Type.GetProperty("Key");
                var valueProperty = typedValue.Type.GetProperty("Value");

                var keyValue = new TypedValue() { Name = keyProperty.Name, Value = keyProperty.GetValue(typedValue.Value) };
                if (keyValue.Value != null) { keyValue.Type = keyValue.Value.GetType(); }
                var valueValue = new TypedValue() { Name = valueProperty.Name, Value = valueProperty.GetValue(typedValue.Value) };
                if (valueValue.Value != null) { valueValue.Type = valueValue.Value.GetType(); }

                return new List<TypedValue>() { keyValue, valueValue };
            };
            return keyValuePairSerializer;
        }

        /// <summary>
        /// Sets the serializr for the related types.
        /// </summary>
        /// <param name="serializer">The serializer in which to set the type serializers.</param>
        public void SetSerializerTypes(JsonSerializer serializer)
        {
            var typeSerializer = MakeSerializer();
            serializer.SetTypeSerializer(typeof(KeyValuePair<,>), typeSerializer);
        }
    }
}
