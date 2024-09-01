#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Topology;
using HularionText.Language.Json.Elements;
using HularionText.StringCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionText.Language.Json
{
    /// <summary>
    /// An abstracted form of a JSON object. 
    /// </summary>
    public class JsonDocument
    {
        /// <summary>
        /// The root container object of the document. This is not a proper JSON object itself.
        /// </summary>
        public JsonRoot Root { get; set; } = new JsonRoot();

        /// <summary>
        /// Modifies the name case for JSON object names.
        /// </summary>
        public StringCaseModifier NameCaseModifier { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nameCaseModifier">Modifies the name case for JSON object names.</param>
        public JsonDocument(StringCaseModifier nameCaseModifier)
        {
            NameCaseModifier = nameCaseModifier;
        }

        /// <summary>
        /// Converts this document to a structure of dictionaries and lists.
        /// </summary>
        /// <returns>The anonymized document.</returns>
        public object MakeAnonymous()
        {
            return Root.MakeAnonymous();
        }

        /// <summary>
        /// Converts this document to a JSON string.
        /// </summary>
        /// <param name="spacing">An option to set the whitespace format of the result.</param>
        /// <returns>A JSON string.</returns>
        public string ToDocumentString(JsonSerializationSpacing spacing = JsonSerializationSpacing.Minimized)
        {
            var result = new StringBuilder();
            var traverser = new TreeTraverser<JsonElement>();

            //There could be duplicate elements, so abstract to ElementProxy to prevent traverser error.
            var proxyTraverser = new TreeTraverser<ElementProxy>();
            var proxyRoot = new ElementProxy() { Element = Root };
            proxyTraverser.CreateEvaluationPlan(TreeTraversalOrder.ParentLeftRight, proxyRoot, node =>
            {
                if (node.Element.ElementType == JsonElementType.Root) { node.Next = ((JsonRoot)node.Element).Values.Select(x=>new ElementProxy() { Element = x }).ToList(); }
                if (node.Element.ElementType == JsonElementType.Object) { node.Next = ((JsonObject)node.Element).Values.Select(x => new ElementProxy() { Element = x }).ToList(); }
                if (node.Element.ElementType == JsonElementType.Array) { node.Next = ((JsonArray)node.Element).Values.Select(x => new ElementProxy() { Element = x }).ToList(); }
                return node.Next.ToArray();
            }, true);

            Func<string, bool, string> stringPrinter = (value, addQuotes) =>
            {
                var printResult = new StringBuilder((int)(1.2 * value.Length));
                if (addQuotes) { printResult.Append('"'); }
                for(var i = 0; i < value.Length; i++)
                {
                    var c = value[i];
                    if (c == '"') { printResult.Append("\\\""); continue; }
                    if (c == '\\') { printResult.Append("\\\\"); continue; }
                    if (c == '/') { printResult.Append("\\/"); continue; }
                    if (c == '\b') { printResult.Append("\\b"); continue; }
                    if (c == '\f') { printResult.Append("\\f"); continue; }
                    if (c == '\n') { printResult.Append("\\n"); continue; }
                    if (c == '\r') { printResult.Append("\\r"); continue; }
                    if (c == '\t') { printResult.Append("\\t"); continue; }
                    if((int)c > 255) 
                    { 
                        printResult.Append("\\u");
                        var chars = new char[] { (char)((c >> 12) & 15), (char)((c >> 8) & 15), (char)((c >> 4) & 15), (char)(c & 15) };
                        for(var j = 0; j < chars.Length; j++)
                        {
                            var cj = chars[j];
                            if (cj < 10) { cj = (char)(48 + cj); }
                            else { cj = (char)(87 + (cj)); }
                            printResult.Append(cj);
                        }
                        continue; 
                    }
                    printResult.Append(c);
                }
                if (addQuotes) { printResult.Append('"'); }
                return printResult.ToString();
            };

            Action<TreeTraverser<ElementProxy>.NodeState> printEntryStart = (state) => { };
            Action<TreeTraverser<ElementProxy>.NodeState> printEntryEnd = (state) => { };
            Action<TreeTraverser<ElementProxy>.NodeState> printExitStart = (state) => { };
            Action<TreeTraverser<ElementProxy>.NodeState> printExitEnd = (state) => { };
            Action<TreeTraverser<ElementProxy>.NodeState> printUpStart = (state) => { };
            Action<TreeTraverser<ElementProxy>.NodeState> printUpEnd = (state) => { };
            Action<TreeTraverser<ElementProxy>.NodeState> printLastStart = (state) => { };
            Action<TreeTraverser<ElementProxy>.NodeState> printLastEnd = (state) => { };
            Action<TreeTraverser<ElementProxy>.NodeState> printValue = (state) =>
            {
                if (state.Parent.Element.ElementType == JsonElementType.Object)
                {
                    result.Append('"');
                    result.Append(state.Subject.Element.Name.Replace(@"\", @"\\"));
                    result.Append('"');
                    result.Append(':');
                    //result.Append(String.Format("\"{0}\":", state.Subject.Element.Name));
                }
                
                if(state.Subject.Element.ElementType == JsonElementType.String)
                {
                    result.Append(stringPrinter(state.Subject.Element.GetStringValue(false), true));
                }
                else
                {
                    result.Append(state.Subject.Element.GetStringValue(false));
                }
                if (!state.IsRowLast) { result.Append(','); }
            };
            Action<TreeTraverser<ElementProxy>.NodeState> printObjectStart = (state) =>
            {
                if (state.Parent.Element.ElementType == JsonElementType.Object) 
                {
                    result.Append('"');
                    result.Append(state.Subject.Element.Name.Replace(@"\", @"\\"));
                    result.Append('"');
                    result.Append(":{");
                    //result.Append(String.Format("\"{0}\":{{", state.Subject.Element.Name)); 
                }
                else { result.Append('{'); }
            };
            Action<TreeTraverser<ElementProxy>.NodeState> printObjectEnd = (state) =>
            {
                result.Append('}');
                if (!state.IsRowLast && state.Parent != null && (state.Parent.Element.ElementType == JsonElementType.Object || state.Parent.Element.ElementType == JsonElementType.Array)) { result.Append(','); }
            };
            Action<TreeTraverser<ElementProxy>.NodeState> printArrayStart = (state) =>
            {
                if (state.Parent.Element.ElementType == JsonElementType.Object)
                {
                    result.Append('"');
                    result.Append(state.Subject.Element.Name.Replace(@"\", @"\\"));
                    result.Append('"');
                    result.Append(":[");
                    //result.Append(String.Format("\"{0}\":[", state.Subject.Element.Name)); 
                }
                else { result.Append('['); }
            };
            Action<TreeTraverser<ElementProxy>.NodeState> printArrayEnd = (state) =>
            {
                result.Append(']');
                if (!state.IsRowLast && state.Parent != null && (state.Parent.Element.ElementType == JsonElementType.Object || state.Parent.Element.ElementType == JsonElementType.Array)) { result.Append(','); }
            };

            if (spacing == JsonSerializationSpacing.Expanded)
            {
                printEntryStart = (state) =>
                {
                    if (state.Depth > 0) { result.Append('\n'); }
                    for (int i = 1; i < state.Depth; i++) { result.Append('\t'); }
                };
                printEntryEnd = (state) => {
                };
                printExitStart = (state) => { };
                printExitEnd = (state) => {
                    if (state.IsRowLast) 
                    { 
                        result.Append('\n');
                        for (int i = 2; i < state.Depth; i++) { result.Append('\t'); }
                    }
                };
            }

            proxyTraverser.WeaveExecute(TreeWeaveOrder.FromLeft, proxyRoot, node=>
            {
                return node.Next.ToArray();
            },
            entryAction: state =>
            {
                printEntryStart(state);
                if (state.Subject.Element.ElementType == JsonElementType.Object) { printObjectStart(state); }
                if (state.Subject.Element.ElementType == JsonElementType.Array) { printArrayStart(state); }
                if (state.Subject.Element.ElementType == JsonElementType.Bool 
                    || state.Subject.Element.ElementType == JsonElementType.Number 
                    || state.Subject.Element.ElementType == JsonElementType.String 
                    || state.Subject.Element.ElementType == JsonElementType.Unknown)
                {
                    printValue(state);
                }
                printEntryEnd(state);
            },
            exitAction: state =>
            {
                printExitStart(state);
                if (state.Subject.Element.ElementType == JsonElementType.Object) { printObjectEnd(state); }
                if (state.Subject.Element.ElementType == JsonElementType.Array) { printArrayEnd(state); }
                printExitEnd(state);
            },
            upAction: state =>
            {
                printUpStart(state);
                printUpEnd(state);
            },
            lastAction: state =>
            {
                printLastStart(state);
                printLastEnd(state);
            });
            return result.ToString();
        }


        public object[] GetPathValues(string[] path)
        {
            path = GetModifiedPath(path);
            var result = new object[Root.Values.Count];
            for (var i = 0; i < Root.Values.Count; i++)
            {
                result[i] = GetIndexPathValue(0, path);
            }
            return result;
        }

        public T[] GetPathValues<T>(string[] path)
        {
            path = GetModifiedPath(path);
            var result = new T[Root.Values.Count];
            for (var i = 0; i < Root.Values.Count; i++)
            {
                var value = GetIndexPathValue(0, path);
                if (value == null) { result[i] = default(T); }
                else { result[i] = (T)value; }
            }
            return result;
        }

        public object GetFirstPathValue(string[] path)
        {
            path = GetModifiedPath(path);
            if (Root.Values.Count == 0) { return null; }
            return GetIndexPathValue(0, path);
        }

        public T GetFirstPathValue<T>(string[] path)
        {
            path = GetModifiedPath(path);
            if (Root.Values.Count == 0) { return default(T); }
            var value = GetIndexPathValue(0, path);
            if(value == null) { return default(T); }
            return (T)GetIndexPathValue(0, path);
        }

        public string[] GetModifiedPath(string[] path)
        {
            if(path == null || path.Length == 0 || NameCaseModifier == null || NameCaseModifier.CaseDefinition == StringCaseDefinition.Original) { return path; }
            var result = new string[path.Length];
            for(var i = 0; i < path.Length; i++)
            {
                result[i] = NameCaseModifier.ModifyCase(path[i]);
            }
            return result;
        }

        private object GetIndexPathValue(int index, string[] path)
        {
            object result = null;
            if(index > Root.Values.Count) { return null; }
            var element = Root.Values[index];
            for(var i = 0; i < path.Length; i++)
            {
                switch (element.ElementType)
                {
                    case JsonElementType.Object:
                        var jo = (JsonObject)element;
                        element = jo.Values.Where(x => x.Name == path[i]).FirstOrDefault();
                        result = element;
                        break;
                    case JsonElementType.Array:
                        var ja = (JsonArray)element;
                        int ji = 0;
                        if(!int.TryParse(path[i], out ji)) { return null; }
                        element = ja.Values[ji];
                        result = element;
                        break;
                }
                if(element == null) { return null; }
            }
            if(element != null)
            {
                switch (element.ElementType)
                {
                    case JsonElementType.Bool:
                        result = ((JsonBool)element).GetValue();
                        break;
                    case JsonElementType.Number:
                        result = ((JsonNumber)element).GetValue();
                        break;
                    case JsonElementType.String:
                        result = ((JsonString)element).GetValue();
                        break;
                }
            }
            return result;
        }

        private class ElementProxy
        {
            public JsonElement Element { get; set; }

            public List<ElementProxy> Next { get; set; } = new List<ElementProxy>();
        }
    }










}
