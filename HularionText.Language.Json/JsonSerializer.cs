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
using HularionCore.Pattern.Topology;
using HularionCore.TypeGraph;
using HularionText.Language.Json.Elements;
using HularionText.Language.Json.Serializers;
using HularionText.StringCase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HularionText.Language.Json
{
    /// <summary>
    /// Serializes typed objects to and from JsonDocument objects.
    /// </summary>
    public class JsonSerializer
    {

        private Dictionary<Type, TypeSerializer> typeSerializers = new Dictionary<Type, TypeSerializer>();

        private IParameterizedProvider<Type, TypeSerializer> TypeSerializers { get; set; }
        private TypeNodeManager TypeNodeManager;

        private JsonLanguage jsonLanguage = new JsonLanguage();

        public StringCaseModifier MemberNameCaseModifier { get; private set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="memberNameModifier">The name modifier for comparing JSON object members to type.</param>
        public JsonSerializer(StringCaseModifier memberNameModifier)
        {
            MemberNameCaseModifier = memberNameModifier;
            TypeNodeManager = new TypeNodeManager(MemberNameCaseModifier.CaseTransform);
            if (MemberNameCaseModifier == null) { MemberNameCaseModifier = new StringCaseModifier(StringCaseDefinition.Original); }
            InitializeTypeSerializers();
        }

        public JsonSerializer()
        {
            MemberNameCaseModifier = new StringCaseModifier(StringCaseDefinition.Original);
            TypeNodeManager = new TypeNodeManager(MemberNameCaseModifier.CaseTransform);
            InitializeTypeSerializers();
        }

        /// <summary>
        /// Serializes the given value to a JsonDocument, and then serializes the JsonDocument to a string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The serialized value.</returns>
        public string Serialize(object value, JsonSerializationSpacing spacing = JsonSerializationSpacing.Minimized)
        {
            var jsonDocument = SerializeToJsonDocument(value);
            if(MemberNameCaseModifier.CaseDefinition == StringCaseDefinition.Original)
            {
                return jsonDocument.ToDocumentString(spacing);
            }
            //return jsonDocument.ToDocumentString(spacing);

            var traverser = new TreeTraverser<JsonElement>();
            var root = new JsonObject()
            {
                Values = jsonDocument.Root.Values.Where(x => x.ElementType == JsonElementType.Object).ToList()
            };
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node =>
            {
                var next = new List<JsonElement>();
                if(node.ElementType == JsonElementType.Object)
                {
                    foreach(var nextNode in ((JsonObject)node).Values)
                    {
                        if(nextNode.ElementType == JsonElementType.Object)
                        {
                            next.Add(nextNode);
                        }
                    }
                }
                return next.ToArray();
            }, true);

            foreach(JsonObject node in plan)
            {
                if (node.MemberNameCaseIsPersistent) { continue; }
                foreach(var element in node.Values)
                {
                    element.Name = MemberNameCaseModifier.CaseModifier(element.Name);
                }
            }

            return jsonDocument.ToDocumentString(spacing);
        }

        /// <summary>
        /// Serializes the given value to a JsonDocument.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The serialized value.</returns>
        public JsonDocument SerializeToJsonDocument(object value)
        {
            if (value == null) { return new JsonDocument(MemberNameCaseModifier); }
            var type = value.GetType();

            var traverser = new TreeTraverser<SerializationRequest>();
            var root = new SerializationRequest() { Serializer = TypeSerializers.Provide(type), TypedValue = new TypedValue() { Type = type, Value = value } };
            var jsonDocument = new JsonDocument(MemberNameCaseModifier) { Root = new JsonRoot() };
            var objects = new Dictionary<object, SerializationRequest>();
            objects.Add(value, root);
            var plan = traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node =>
            {
                if(node.TypedValue.Value == null) 
                { 
                    node.Element = new JsonUnknown();
                    node.Element.Name = node.TypedValue.Name;
                    return new SerializationRequest[] { }; 
                }

                node.Element = node.Serializer.Serialize(new SerializationDetail() { TypedValue = node.TypedValue, TypeNodeProvider = TypeNodeManager.TypeNodeProvider });
                if (node.TypedValue != null) { node.Element.Name = node.TypedValue.Name; }
                var nextValues = node.Serializer.GetNextValues(node.TypedValue);
                var nextNodes = new List<SerializationRequest>();
                foreach (var nextValue in nextValues)
                {
                    if (nextValue.Type == typeof(object) && nextValue.Value != null) { nextValue.Type = nextValue.Value.GetType(); }
                    var serializer = TypeSerializers.Provide(nextValue.Type);
                    var next = new SerializationRequest() { Serializer = serializer, TypedValue = nextValue };
                    node.Next.Add(next);
                    if (nextValue.Value != null && !objects.ContainsKey(nextValue.Value)) { objects.Add(nextValue.Value, next); }
                    if (nextValue.Value == null || objects.ContainsKey(nextValue.Value)) { nextNodes.Add(next); }
                }
                return nextNodes.ToArray();
            }, true);
            
            traverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, root, node =>
            {
                if (node.Element.ElementType == JsonElementType.Object)
                {
                    ((JsonObject)node.Element).Values = node.Next.Select(x => x.Element).ToList();
                }
                if (node.Element.ElementType == JsonElementType.Array)
                {
                    ((JsonArray)node.Element).Values = node.Next.Select(x => x.Element).ToList();
                }
                return node.Next.ToArray();
            }, true);

            jsonDocument.Root.AddNode(root.Element);
            return jsonDocument;
        }

        /// <summary>
        /// Deserializes the text to a JsonDocument, and then deserializes the JsonDocument to an object of the provided type.
        /// </summary>
        /// <typeparam name="T">The type to which the text is deserialized.</typeparam>
        /// <param name="text">The text to deserialize.</param>
        /// <returns>The typed object containing the information in text.</returns>
        public T Deserialize<T>(string text)
        {
            return DeserializeAll<T>(text).FirstOrDefault();
        }

        /// <summary>
        /// Deserializes the text to a JsonDocument, and then deserializes the JsonDocument to an object of the provided type.
        /// </summary>
        /// <param name="type">The type to which the text is deserialized.</param>
        /// <param name="text">The text to deserialize.</param>
        /// <returns>The typed object containing the information in text.</returns>
        public object Deserialize(Type type, string text)
        {
            return DeserializeAll(type, text).FirstOrDefault();
        }

        /// <summary>
        /// Deserializes the text to zero or more JsonDocument objects, and then deserializes the JsonDocument objects to an object of the provided type.
        /// </summary>
        /// <typeparam name="T">The type to which the text is deserialized.</typeparam>
        /// <param name="text">The text to deserialize.</param>
        /// <returns>The typed objects containing the information in text.</returns>
        public T[] DeserializeAll<T>(string text)
        {
            var result = DeserializeAll(typeof(T), text);
            return result.Select(x => (T)x).ToArray();
        }

        /// <summary>
        /// Deserializes the text to zero or more JsonDocument objects, and then deserializes the JsonDocument objects to an object of the provided type.
        /// </summary>
        /// <param name="type">The type to which the text is deserialized.</param>
        /// <param name="text">The text to deserialize.</param>
        /// <returns>The typed objects containing the information in text.</returns>
        public object[] DeserializeAll(Type type, string text)
        {
            var jsonDocument = jsonLanguage.Parse(text, MemberNameCaseModifier);
            return DeserializeAllFromJsonDocument(jsonDocument, type);
        }

        /// <summary>
        /// Deserializes the json document to values of the provided type.
        /// </summary>
        /// <param name="jsonDocument">The document to deserialize.</param>
        /// <param name="type">The type to which the json document root values will be deserialized.</param>
        /// <returns>An array of objects deserialized from jsonDocument.</returns>
        public object[] DeserializeAllFromJsonDocument(JsonDocument jsonDocument, Type type)
        {
            var typeNode = TypeNodeManager.TypeNodeProvider.Provide(type);
            var requestTraverser = new TreeTraverser<DeserializationRequest>();

            var tops = new List<DeserializationRequest>();
            foreach (var topElement in jsonDocument.Root.Values)
            {
                var top = new DeserializationRequest() { Element = topElement, Type = typeNode };
                tops.Add(top);
                var valueMap = new Dictionary<JsonElement, object>();

                var plan = requestTraverser.CreateEvaluationPlan(TreeTraversalOrder.LeftRightParent, top, node =>
                {
                    var serializer = TypeSerializers.Provide(node.Type.Type);
                    if (serializer.GetNextDeserializationRequests != null)
                    {
                        node.TypeNodeProvider = TypeNodeManager.TypeNodeProvider;
                        return serializer.GetNextDeserializationRequests(node).ToArray();
                    }
                    var next = new List<DeserializationRequest>();
                    var nextElements = node.Element.GetNextNodes();
                    foreach (var nextElement in nextElements)
                    {
                        //var name = nextElement.Name.ToLower();
                        if (node.Element.ElementType == JsonElementType.Object && !node.Type.Members.ContainsKey(nextElement.Name)) { continue; }
                        var nextNode = new DeserializationRequest() { Element = nextElement };
                        if (node.Element.ElementType == JsonElementType.Object)
                        {
                            nextNode.Type = node.Type.Members[nextElement.Name].Member;
                        }
                        if (node.Element.ElementType == JsonElementType.Array)
                        {
                            if (node.Type.Generics.Count() == 0) { continue; }
                            nextNode.Type = node.Type.Generics.First();
                        }
                        next.Add(nextNode);
                    }
                    return next.ToArray();
                }, true);

                foreach (var node in plan)
                {
                    var serializer = TypeSerializers.Provide(node.Type.Type);
                    var detail = new SerializationDetail() { Element = node.Element, TypedValue = new TypedValue() { Type = node.Type.Type }, ValueMap = valueMap };
                    node.DeserializedValue = serializer.Deserialize(detail);
                    valueMap.Add(node.Element, node.DeserializedValue);
                }
            }
            return tops.Select(x => x.DeserializedValue).ToArray();
        }

        /// <summary>
        /// Deserializes jsonDocument into an array of objects of the provided type.
        /// </summary>
        /// <typeparam name="T">The type to which the text is deserialized.</typeparam>
        /// <param name="jsonDocument">The document to deserialize.</param>
        /// <returns>The typed objects containing the information in jsonDocument.</returns>
        public T[] DeserializeAllFromJsonDocument<T>(JsonDocument jsonDocument)
        {
            var result = DeserializeAllFromJsonDocument(jsonDocument, typeof(T));
            return result.Select(x => (T)x).ToArray();
        }

        /// <summary>
        /// Deserializes jsonDocument and returns the object of the provided type.
        /// </summary>
        /// <typeparam name="T">The type to which the jsonDocument is deserialized.</typeparam>
        /// <param name="jsonDocument">The document to deserialize.</param>
        /// <returns>The typed object containing the information in jsonDocument.</returns>
        public T DeserializeFromJsonDocument<T>(JsonDocument jsonDocument)
        {
            return DeserializeAllFromJsonDocument<T>(jsonDocument).FirstOrDefault();
        }

        /// <summary>
        /// Deserializes jsonDocument and returns the object of the provided type.
        /// </summary>
        /// <param name="jsonDocument">The document to deserialize.</param>
        /// <param name="type"The type to which the jsonDocument is deserialized.</param>
        /// <returns>The typed object containing the information in jsonDocument.</returns>
        public object DeserializeFromJsonDocument(JsonDocument jsonDocument, Type type)
        {
            return DeserializeAllFromJsonDocument(jsonDocument, type).FirstOrDefault();
        }

        /// <summary>
        /// Creates a JsonDocument given the json text.
        /// </summary>
        /// <param name="text">The json text.</param>
        /// <returns>A JsonDocument corresponding to text.</returns>
        public JsonDocument ToJsonDocument(string text)
        {
            var jsonDocument = jsonLanguage.Parse(text, MemberNameCaseModifier);
            return jsonDocument;
        }

        /// <summary>
        /// Sets the serializer for the specified type. If the type is a generic definition, all manifested types (including generics) will use this serializer unless that type is also set separately.
        /// </summary>
        /// <typeparam name="T">The type for which the serializer is set.</typeparam>
        /// <param name="serializer">The serializer to set.</param>
        public void SetTypeSerializer<T>(TypeSerializer serializer)
        {
            SetTypeSerializer(typeof(T), serializer);
        }

        /// <summary>
        /// Sets the serializer for the specified type. If the type is a generic definition, all manifested types (including generics) will use this serializer unless that type is also set separately.
        /// </summary>
        /// <param name="type">The type for which the serializer is set.</param>
        /// <param name="serializer">The serializer to set.</param>
        public void SetTypeSerializer(Type type, TypeSerializer serializer)
        {
            lock (typeSerializers)
            {
                typeSerializers[type] = serializer;
            }
        }

        private TypeSerializer MakeSerializer<T>(Func<SerializationDetail, JsonElement> serializer, Func<SerializationDetail, object> deserializer, JsonElementType elementType, Func<TypedValue, IEnumerable<TypedValue>> getNext = null, bool addToSerializers = true)
        {
            return MakeSerializer(typeof(T), serializer, deserializer, elementType, getNext:getNext, addToSerializers: addToSerializers);
        }

        private TypeSerializer MakeSerializer(Type type, Func<SerializationDetail, JsonElement> serializer, Func<SerializationDetail, object> deserializer, JsonElementType elementType, Func<TypedValue, IEnumerable<TypedValue>> getNext = null, bool addToSerializers = true)
        {
            var result = new TypeSerializer()
            {
                Serialize = serializer,
                Deserialize = deserializer
            };
            if (getNext != null) { result.GetNextValues = getNext; }
            if (addToSerializers) { typeSerializers.Add(type, result); }
            return result;
        }

        private TypeSerializer MakeNumberSerializer<T>(Func<string, T> parse)
        {
            var type = typeof(T);
            var serializer = MakeSerializer(type,
                detail => new JsonNumber() { Value = detail.TypedValue.Value == null ? null : detail.TypedValue.Value.ToString() },
                detail =>
                {
                    return parse(((JsonNumber)detail.Element).Value);
                },
                JsonElementType.Number);
            return serializer;
        }

        private void InitializeTypeSerializers()
        {
            MakeSerializer<object>(detail =>
            { 
                var result = new JsonUnknown();
                if (detail.TypedValue.Value == null) { result.Value = null; }
                else if (detail.TypedValue.Value.GetType() == typeof(string)) { result.Value = String.Format("\"{0}\"", detail.TypedValue.Value); }
                else if (detail.TypedValue.Value.GetType() == typeof(bool)) { result.Value = String.Format("{0}", detail.TypedValue.Value).ToLower(); }
                else { result.Value = String.Format("\"{0}\"", detail.TypedValue.Value.ToString()); }
                var typeNode = TypeNodeManager.TypeNodeProvider.Provide(detail.TypedValue.Value.GetType());
                return result;
            }, detail => null, JsonElementType.Unknown);
            MakeSerializer<string>(detail => new JsonString() { Value = (string)detail.TypedValue.Value }, detail => detail.Element.GetValue(), JsonElementType.String);
            MakeSerializer<char>(detail => new JsonString() { Value = String.Format("{0}", detail.TypedValue.Value) }, detail => detail.Element.GetStringValue(false).FirstOrDefault(), JsonElementType.String);
            MakeSerializer<bool>(detail => new JsonBool() { Value = (bool)detail.TypedValue.Value }, detail => 
            {
                var value = detail.Element.GetValue();
                if(value == null || value.GetType() != typeof(bool)) { return default(bool); }
                return (bool)value; 
            }, JsonElementType.Bool);
            MakeSerializer<DateTime>(detail => new JsonString() { Value = String.Format("{0}", detail.TypedValue.Value) }, detail => DateTime.Parse(String.Format("{0}", detail.Element.GetValue())), JsonElementType.String);
            MakeSerializer<byte[]>(
                detail => 
                {
                    return new JsonString() { Value = Convert.ToBase64String((byte[])detail.TypedValue.Value) };
                }, 
                detail =>
                {
                    var value = detail.Element.GetStringValue();
                    if (String.IsNullOrWhiteSpace(value)) { return null; }
                    return Convert.FromBase64String(value);
                }, JsonElementType.String);

            MakeNumberSerializer<byte>(s=>byte.Parse(s));
            MakeNumberSerializer<short>(s => short.Parse(s));
            MakeNumberSerializer<int>(s => int.Parse(s));
            MakeNumberSerializer<long>(s => long.Parse(s));
            MakeNumberSerializer<ushort>(s => ushort.Parse(s));
            MakeNumberSerializer<uint>(s => uint.Parse(s));
            MakeNumberSerializer<ulong>(s => ulong.Parse(s));
            MakeNumberSerializer<float>(s => float.Parse(s));
            MakeNumberSerializer<double>(s => double.Parse(s));
            MakeNumberSerializer<decimal>(s => decimal.Parse(s));

            (new CollectionSerializer()).SetSerializerTypes(this);
            (new DictionarySerializer()).SetSerializerTypes(this);
            (new KeyValuePairSerializer()).SetSerializerTypes(this);

            TypeSerializers = ParameterizedProvider.FromSingle<Type, TypeSerializer>(type => 
            {
                if(type == null) { return null; }
                if (typeSerializers.ContainsKey(type)) { return typeSerializers[type]; }
                if (type.IsGenericType)
                {
                    var genericType = type.GetGenericTypeDefinition();
                    if (typeSerializers.ContainsKey(genericType)) { return typeSerializers[genericType]; }
                }

                lock (typeSerializers)
                {
                    var processType = type;
                    if (type.IsGenericType) { processType = type.GetGenericTypeDefinition(); }
                    if(type.IsArray) { processType = typeof(Array); }
                    if (typeSerializers.ContainsKey(processType)) { return typeSerializers[processType]; }

                    if (type.IsEnum)
                    {
                        var enumMap = new Dictionary<string, object>();
                        foreach(var enumValue in type.GetEnumValues())
                        {
                            enumMap.Add(enumValue.ToString(), enumValue);
                        }
                        var serializer = MakeSerializer(type, detail => new JsonString() { Value = String.Format("{0}", detail.TypedValue.Value) },
                            detail =>
                            {
                                var value = enumMap[((JsonString)detail.Element).Value];
                                return value;
                            }, 
                            JsonElementType.String);
                    }
                    else if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
                    {
                        TypeSerializer serializer = MakeSerializer(processType, null, null, JsonElementType.Object, addToSerializers: false);
                        serializer.Serialize = detail => new JsonObject();
                        serializer.Deserialize = detail =>
                        {
                            if (detail.Element.ElementType != JsonElementType.Object) { return null; }
                            var typeNode = TypeNodeManager.TypeNodeProvider.Provide(detail.TypedValue.Type);
                            var value = Activator.CreateInstance(detail.TypedValue.Type);
                            if(value != null)
                            {
                                DeserializeJsonObjectMembers(detail, value);
                            }
                            //var namedValues = ((JsonObject)detail.Element).Values.ToDictionary(x => x.Name, x => detail.ValueMap.ContainsKey(x) ?  detail.ValueMap[x] : null);
                            //foreach (var item in namedValues)
                            //{
                            //    //if(item.Value == null) { continue; }
                            //    if (!typeNode.Members.ContainsKey(item.Key)) 
                            //    {
                            //        continue; 
                            //    }
                            //    typeNode.Members[item.Key].SetMemberValue(value, item.Value);
                            //}
                            return value;
                        };
                        serializer.GetNextValues = CreateStandardGetNextValues();
                        typeSerializers.Add(type, serializer);
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("The serializer for type '{0}' could not be determined. Register the serializer or provide a default construtor. [Z7AhyuNo6kSul3PGhCIfOg]", type.Name));
                    }
                }
                return typeSerializers[type];
            });
        }

        /// <summary>
        /// Creates a standard Func that will return the next values of the typed value.
        /// </summary>
        /// <returns>A standard Func that will return the next values of the typed value.</returns>
        public Func<TypedValue, IEnumerable<TypedValue>> CreateStandardGetNextValues()
        {
            return typedValue =>
            {
                var typeNode = TypeNodeManager.TypeNodeProvider.Provide(typedValue.Type);
                var result = new List<TypedValue>();
                foreach (var member in typeNode.Members)
                {
                    var memberValue = member.Value.GetMemberValue(typedValue.Value);
                    result.Add(new TypedValue() { Type = member.Value.Type, Value = memberValue, Name = member.Key });
                }
                return result;
            };
        }

        /// <summary>
        /// Deserializes a JsonObject given in "detail" to "value". A helper for custom deserializers.
        /// </summary>
        /// <param name="detail">The SerializationDetail.</param>
        /// <param name="value">The object being deserialized to.</param>
        /// <param name="memberNames">If not null, only the names of given members will be deserialized.</param>
        public void DeserializeJsonObjectMembers(SerializationDetail detail, object value, IEnumerable<string> memberNames = null)
        {
            var typeNode = TypeNodeManager.TypeNodeProvider.Provide(detail.TypedValue.Type);
            var elements = ((JsonObject)detail.Element).Values;

            foreach (var element in elements)
            {
                if(memberNames != null && !memberNames.Contains(element.Name)) 
                { 
                    continue; 
                }
                if (!typeNode.Members.ContainsKey(element.Name))
                {
                    continue;
                }
                if (detail.ValueMap.ContainsKey(element))
                {
                    typeNode.Members[element.Name].SetMemberValue(value, detail.ValueMap[element]);
                }
            }

        }


        internal class SerializationRequest
        {

            public TypedValue TypedValue { get; set; }

            public TypeSerializer Serializer { get; set; }

            public JsonElement Element { get; set; }

            public List<SerializationRequest> Next { get; set; } = new List<SerializationRequest>();

            public override string ToString()
            {
                return String.Format("JsonSerializeRequest: {0} {1}", Element == null ? string.Empty : Element.ElementType.ToString(), TypedValue.Value);
            }
        }

    }

}
