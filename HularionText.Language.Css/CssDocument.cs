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

namespace HularionText.Language.Css
{
    public class CssDocument
    {

        public List<CssBlock> Blocks { get; set; } = new List<CssBlock>();


        public void AddBlock(CssBlock block)
        {
            Blocks.Add(block);
        }

        public void PrefixBlockSelectors(string prefix)
        {
            foreach(var block in Blocks) 
            {
                block.AddSelectorPrefix(prefix);
            }
        }

        /// <summary>
        /// Replaces all instances of search in each block selector with replacement.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="replacement">The string to replace search with.</param>
        public void ReplaceBlockSelectorMatch(string search, string replacement)
        {
            foreach (var block in Blocks)
            {
                for(var i = 0; i < block.SelectorParts.Count; i++)
                {
                    var part = block.SelectorParts[i];
                    if (part == search)
                    {
                        block.SelectorParts[i] = replacement;
                    }
                }
            }
        }

        public string ToDocumentString()
        {
            var result = new StringBuilder();

            foreach(var block in Blocks)
            {
                foreach(var selector in block.SelectorParts) { result.Append(selector); }
                result.Append(" {\n");
                foreach (var line in block.BlockEntries)
                {
                    result.Append("\t");
                    result.Append(line.Property);
                    result.Append(":");
                    result.Append(line.Value);
                    result.Append("\n");
                }
                result.Append("}\n");
            }

            return result.ToString();
        }

    }



}
