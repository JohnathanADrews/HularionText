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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionText.Language.Css
{
    public class CssBlock
    {
        //public string SelectorPrefix { get; set; }

        public string Selector 
        { 
            get
            {
                var result = new StringBuilder();
                foreach(var part in SelectorParts)
                {
                    result.Append(part);
                }
                return result.ToString();
            }
        }

        public List<string> SelectorParts { get; set; } = new List<string>();

        public List<CssBlockEntry> BlockEntries { get; set; } = new List<CssBlockEntry>();

        /// <summary>
        /// Prefixes the selector with the indicated prefix.
        /// </summary>
        /// <param name="prefix">The prefix to use.</param>
        public void AddSelectorPrefix(string prefix)
        {
            SelectorParts.Insert(0, prefix);
        }

        /// <summary>
        /// Replaces each part matching the search string with the replacement string.
        /// </summary>
        /// <param name="search">The string to find.</param>
        /// <param name="replacement">The string to replace the match.</param>
        public void ReplaceSelectorPart(string search, string replacement)
        {
            for (var i = 0; i < SelectorParts.Count; i++)
            {
                var part = SelectorParts[i];
                if (part == search)
                {
                    SelectorParts[i] = replacement;
                }
            }
        }

        public string Value
        {
            get
            {
                var result = new StringBuilder();
                result.Append(Selector.Trim());
                result.Append(" {\n");
                foreach (var entry in BlockEntries)
                {
                    result.Append("\t");
                    result.Append(entry.ToString().Trim());
                    result.Append("\n");
                }
                result.Append("}");
                return result.ToString();
            }
        }

        public override string ToString()
        {
            return Value;
        }

    }
}
