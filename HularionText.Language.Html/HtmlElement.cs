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

namespace HularionText.Language.Html
{
    public class HtmlElement
    {

        public HtmlElementType Type { get; set; } = HtmlElementType.Unknown;

        public HtmlElementClass Class { get; set; } = HtmlElementClass.Unknown;

        public string Text
        {
            get
            {
                if (String.IsNullOrWhiteSpace(text)) { text = Type.ToString().ToLower(); }
                return text;
            }
        }
        private string text;

        public string OpenOperator 
        {
            get 
            {
                if (String.IsNullOrWhiteSpace(openOperator)) { openOperator = String.Format("<{0}", Type.ToString().ToLower()); }
                return openOperator; 
            } 
        }
        private string openOperator;

        public string CloseOperator
        {
            get
            {
                if (String.IsNullOrWhiteSpace(closeOperator)) { closeOperator = String.Format("</{0}", Type.ToString().ToLower()); }
                return closeOperator;
            }
        }
        private string closeOperator;

        public bool OpensContext { get; set; } = false;

        public bool ClosesContext { get; set; } = false;

        public bool HasInnerLanguage { get; set; } = false;

        public bool IsTextContainer { get; set; } = false;

        public TextSymbol Symbol { get { if (symbol == null) { symbol = new TextSymbol() { Text = Text, Group = ElementGroup }; } return symbol; } }

        private TextSymbol symbol = null;

        public HtmlElement()
        {
            //   Symbol = new TextSymbol() { Sequence = Text.ToCharArray(), Group = ElementGroup };
        }

        public static SymbolGroup ElementGroup = new SymbolGroup() { Name = "Element" };

        public static IEnumerable<HtmlElement> Elements { get { return typeof(HtmlElement).GetFields().Where(x => x.FieldType == typeof(HtmlElement)).Select(x => (HtmlElement)x.GetValue(null)).ToList(); } }

        private static Dictionary<string, HtmlElement> elementMap;

        static HtmlElement()
        {
            elementMap = Elements.ToDictionary(x => x.Text, x => x);
        }

        public static HtmlElement Parse(string text)
        {
            text = String.Format("{0}", text).ToLower().Trim();
            if (!elementMap.ContainsKey(text)) { return HtmlElement.Unknown; }
            return elementMap[text];
        }

        /// <summary>
        /// Represents text adjacent to elements.
        /// </summary>
        public static HtmlElement FreeText = new HtmlElement() { Class = HtmlElementClass.FreeText, Type = HtmlElementType.FreeText };

        public static HtmlElement Unknown = new HtmlElement() { Class = HtmlElementClass.Unknown, Type = HtmlElementType.Unknown };
        public static HtmlElement Html = new HtmlElement() { Class = HtmlElementClass.Root, Type = HtmlElementType.Html };
        public static HtmlElement Base = new HtmlElement() { Class = HtmlElementClass.Metadata, Type = HtmlElementType.Base };
        public static HtmlElement Head = new HtmlElement() { Class = HtmlElementClass.Metadata, Type = HtmlElementType.Head };
        public static HtmlElement Link = new HtmlElement() { Class = HtmlElementClass.Metadata, Type = HtmlElementType.Link };
        public static HtmlElement Meta = new HtmlElement() { Class = HtmlElementClass.Metadata, Type = HtmlElementType.Meta };
        public static HtmlElement Style = new HtmlElement() { Class = HtmlElementClass.Metadata, Type = HtmlElementType.Style, IsTextContainer = true, HasInnerLanguage = true };
        public static HtmlElement Title = new HtmlElement() { Class = HtmlElementClass.Metadata, Type = HtmlElementType.Title, IsTextContainer = true };
        public static HtmlElement Body = new HtmlElement() { Class = HtmlElementClass.RootSection, Type = HtmlElementType.Body };
        public static HtmlElement Address = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.Address };
        public static HtmlElement Article = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.Article };
        public static HtmlElement Aside = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.Aside };
        public static HtmlElement Footer = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.Footer };
        public static HtmlElement Header = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.Header };
        public static HtmlElement H1 = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.H1, IsTextContainer = true };
        public static HtmlElement H2 = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.H2, IsTextContainer = true };
        public static HtmlElement H3 = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.H3, IsTextContainer = true };
        public static HtmlElement H4 = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.H4, IsTextContainer = true };
        public static HtmlElement H5 = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.H5, IsTextContainer = true };
        public static HtmlElement H6 = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.H6, IsTextContainer = true };
        public static HtmlElement Main = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.Main };
        public static HtmlElement Nav = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.Nav };
        public static HtmlElement Section = new HtmlElement() { Class = HtmlElementClass.ContentSection, Type = HtmlElementType.Section };
        public static HtmlElement BlockQuote = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.BlockQuote };
        public static HtmlElement Dd = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Dd };
        public static HtmlElement Div = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Div };
        public static HtmlElement Dl = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Dl };
        public static HtmlElement Dt = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Dt };
        public static HtmlElement FigCaption = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.FigCaption };
        public static HtmlElement Figure = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Figure };
        public static HtmlElement Hr = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Hr };
        public static HtmlElement Li = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Li };
        public static HtmlElement Ol = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Ol };
        public static HtmlElement P = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.P, IsTextContainer = true };
        public static HtmlElement Pre = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Pre };
        public static HtmlElement Ul = new HtmlElement() { Class = HtmlElementClass.Text, Type = HtmlElementType.Ul };
        public static HtmlElement A = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.A };
        public static HtmlElement Abbr = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Abbr };
        public static HtmlElement B = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.B };
        public static HtmlElement Bdi = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Bdi };
        public static HtmlElement Bdo = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Bdo };
        public static HtmlElement Br = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Br };
        public static HtmlElement Cite = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Cite };
        public static HtmlElement Code = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Code };
        public static HtmlElement Data = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Data };
        public static HtmlElement Dfn = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Dfn };
        public static HtmlElement Em = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Em };
        public static HtmlElement I = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.I };
        public static HtmlElement Kbd = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Kbd };
        public static HtmlElement Mark = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Mark };
        public static HtmlElement Q = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Q };
        public static HtmlElement Rb = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Rb };
        public static HtmlElement Rp = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Rp };
        public static HtmlElement Rt = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Rt };
        public static HtmlElement Rtc = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Rtc };
        public static HtmlElement Ruby = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Ruby };
        public static HtmlElement S = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.S };
        public static HtmlElement Samp = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Samp };
        public static HtmlElement Small = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Small };
        public static HtmlElement Span = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Span };
        public static HtmlElement Strong = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Strong };
        public static HtmlElement Sub = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Sub };
        public static HtmlElement Sup = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Sup };
        public static HtmlElement Time = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Time };
        public static HtmlElement U = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.U };
        public static HtmlElement Var = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Var };
        public static HtmlElement Wbr = new HtmlElement() { Class = HtmlElementClass.InlineText, Type = HtmlElementType.Wbr };
        public static HtmlElement Area = new HtmlElement() { Class = HtmlElementClass.Multimedia, Type = HtmlElementType.Area };
        public static HtmlElement Audio = new HtmlElement() { Class = HtmlElementClass.Multimedia, Type = HtmlElementType.Audio };
        public static HtmlElement Img = new HtmlElement() { Class = HtmlElementClass.Multimedia, Type = HtmlElementType.Img };
        public static HtmlElement Map = new HtmlElement() { Class = HtmlElementClass.Multimedia, Type = HtmlElementType.Map };
        public static HtmlElement Track = new HtmlElement() { Class = HtmlElementClass.Multimedia, Type = HtmlElementType.Track };
        public static HtmlElement Video = new HtmlElement() { Class = HtmlElementClass.Multimedia, Type = HtmlElementType.Video };
        public static HtmlElement Embed = new HtmlElement() { Class = HtmlElementClass.Embedded, Type = HtmlElementType.Embed };
        public static HtmlElement IFrame = new HtmlElement() { Class = HtmlElementClass.Embedded, Type = HtmlElementType.IFrame };
        public static HtmlElement Object = new HtmlElement() { Class = HtmlElementClass.Embedded, Type = HtmlElementType.Object };
        public static HtmlElement Param = new HtmlElement() { Class = HtmlElementClass.Embedded, Type = HtmlElementType.Param };
        public static HtmlElement Picture = new HtmlElement() { Class = HtmlElementClass.Embedded, Type = HtmlElementType.Picture };
        public static HtmlElement Portal = new HtmlElement() { Class = HtmlElementClass.Embedded, Type = HtmlElementType.Portal };
        public static HtmlElement Source = new HtmlElement() { Class = HtmlElementClass.Embedded, Type = HtmlElementType.Source };
        public static HtmlElement Svg = new HtmlElement() { Class = HtmlElementClass.Svg, Type = HtmlElementType.Svg };
        public static HtmlElement Path = new HtmlElement() { Class = HtmlElementClass.Svg, Type = HtmlElementType.Path };
        public static HtmlElement Math = new HtmlElement() { Class = HtmlElementClass.Math, Type = HtmlElementType.Math };
        public static HtmlElement Canvas = new HtmlElement() { Class = HtmlElementClass.Script, Type = HtmlElementType.Canvas };
        public static HtmlElement NoScript = new HtmlElement() { Class = HtmlElementClass.Script, Type = HtmlElementType.NoScript };
        public static HtmlElement Script = new HtmlElement() { Class = HtmlElementClass.Script, Type = HtmlElementType.Script, IsTextContainer = true, HasInnerLanguage = true };
        public static HtmlElement Del = new HtmlElement() { Class = HtmlElementClass.Demarcation, Type = HtmlElementType.Del };
        public static HtmlElement Ins = new HtmlElement() { Class = HtmlElementClass.Demarcation, Type = HtmlElementType.Ins };
        public static HtmlElement Caption = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.Caption };
        public static HtmlElement Col = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.Col };
        public static HtmlElement Colgroup = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.Colgroup };
        public static HtmlElement Table = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.Table };
        public static HtmlElement TBody = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.TBody };
        public static HtmlElement Td = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.Td };
        public static HtmlElement TFoot = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.TFoot };
        public static HtmlElement Th = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.Th };
        public static HtmlElement THead = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.THead };
        public static HtmlElement Tr = new HtmlElement() { Class = HtmlElementClass.Table, Type = HtmlElementType.Tr };
        public static HtmlElement Button = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Button };
        public static HtmlElement DataList = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.DataList };
        public static HtmlElement FieldSet = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.FieldSet };
        public static HtmlElement Form = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Form };
        public static HtmlElement Input = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Input };
        public static HtmlElement Label = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Label, IsTextContainer = true };
        public static HtmlElement Legend = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Legend };
        public static HtmlElement Meter = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Meter };
        public static HtmlElement OptGroup = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.OptGroup };
        public static HtmlElement Option = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Option };
        public static HtmlElement Output = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Output };
        public static HtmlElement Progress = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Progress };
        public static HtmlElement Select = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.Select };
        public static HtmlElement TextArea = new HtmlElement() { Class = HtmlElementClass.Form, Type = HtmlElementType.TextArea, IsTextContainer = true };
        public static HtmlElement Details = new HtmlElement() { Class = HtmlElementClass.Interactive, Type = HtmlElementType.Details };
        public static HtmlElement Dialog = new HtmlElement() { Class = HtmlElementClass.Interactive, Type = HtmlElementType.Dialog };
        public static HtmlElement Menu = new HtmlElement() { Class = HtmlElementClass.Interactive, Type = HtmlElementType.Menu };
        public static HtmlElement Summary = new HtmlElement() { Class = HtmlElementClass.Interactive, Type = HtmlElementType.Summary };
        public static HtmlElement Slot = new HtmlElement() { Class = HtmlElementClass.Component, Type = HtmlElementType.Slot };
        public static HtmlElement Template = new HtmlElement() { Class = HtmlElementClass.Component, Type = HtmlElementType.Template };

    }
}
