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

namespace HularionText.Language.Xml
{
    public class XmlAttribute
    {
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the attribute.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The type of quotes used for the attribute. double/single/none
        /// </summary>
        public AttributeQuoteType QuoteType { get; set; }

        /// <summary>
        /// true iff the attribute uses = to set a value.
        /// </summary>
        public bool HasEquals { get; set; }

        /// <summary>
        /// The node to which this attribute belongs.
        /// </summary>
        public XmlNode Node { get; set; }


        public override string ToString()
        {
            if (HasEquals)
            {
                return String.Format("{0}={2}{1}{2}", Name, Value, QuoteType == AttributeQuoteType.Single ? "'" : "\"");
            }
            return Name;
        }

    }
}
