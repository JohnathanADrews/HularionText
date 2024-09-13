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
using HularionText.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionText.Language.Xml
{
    public class XmlElement
    {
        public string Text { get; set; }

        public string OpenOperator 
        {
            get { return String.Format("<{0}", Text); } 
        }

        public string CloseOperator
        {
            get { return String.Format("</{0}", Text); } 
        }

        public bool IsUnknown { get; set; } = false;

        //public bool OpensContext { get; set; } = false;

        //public bool ClosesContext { get; set; } = false;

        //public bool HasInnerLanguage { get; set; } = false;

        //public bool IsTextContainer { get; set; } = false;

        //public TextSymbol Symbol { get { if (symbol == null) { symbol = new TextSymbol() { Text = Text, Group = ElementGroup }; } return symbol; } }

        //private TextSymbol symbol = null;

        public XmlElement()
        {
        }



    }
}
