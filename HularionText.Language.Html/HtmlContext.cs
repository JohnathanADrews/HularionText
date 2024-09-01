#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionText.Compiler.Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionText.Language.Html
{
    public class HtmlContext : SequenceProcessContext<HtmlDocument>
    {

        public HtmlDocument Document { get; set; } = new HtmlDocument();

        public string Text { get; set; }

        public TextSequenceMatch Match { get; set; }

        public TextSequenceTree TextTree { get; set; }

        public TextSequenceTree StateTree { get; set; }

        public HtmlElement Element { get; set; }

        public HtmlNode Node { get; set; }

        public List<HtmlAttribute> Attributes { get; set; } = new List<HtmlAttribute>();

        public bool IsNodeEnd { get; set; } = false;

        public bool IsAttributeValueStared { get; set; } = false;

        public int ValueStartIndex { get; set; }

        public int ValueEndIndex { get; set; }

        public HtmlAttribute Attribute { get; set; }

        public void SetNodeElementFromSubstring(int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex <= startIndex) 
            {
                Node.Element = HtmlElement.Unknown;
                return; 
            }
            Node.ElementText = Text.Substring(startIndex, endIndex - startIndex);
            Node.Element = HtmlElement.Parse(Node.ElementText);
        }

        public string MatchValue
        {
            get
            {
                return Match.Value.Text;
            }
        }

    }
}
