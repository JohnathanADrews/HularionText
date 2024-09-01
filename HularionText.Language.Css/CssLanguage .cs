#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Structure;
using HularionText.Compiler;
using HularionText.Compiler.Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HularionText.Language.Css
{
    /// <summary>
    /// A language parser for CSS that can convert CSS string into a CssDocument.
    /// </summary>
    public class CssLanguage : HularionLanguage<CssProcessContext, CssOperationMode, CssDocument>
    {


        TextSymbol blockScopeBegin = new TextSymbol() { Text = "{" };
        TextSymbol blockScopeEnd = new TextSymbol() { Text = "}" };

        TextSymbol selectorIdStart = new TextSymbol() { Text = "#" };
        TextSymbol selectorClassStart = new TextSymbol() { Text = "." };
        TextSymbol selectorSingleColon = new TextSymbol() { Text = ":" };
        TextSymbol selectorDoubleColon = new TextSymbol() { Text = "::" };
        TextSymbol selectorPlus = new TextSymbol() { Text = "+" };
        TextSymbol selectorRightAngle = new TextSymbol() { Text = ">" };
        TextSymbol selectorEquals = new TextSymbol() { Text = "=" };
        TextSymbol selectorCarrot = new TextSymbol() { Text = "^" };
        TextSymbol selectorTilde = new TextSymbol() { Text = "~" };
        TextSymbol selectorPipe = new TextSymbol() { Text = "|" };
        TextSymbol selectorDollarSign = new TextSymbol() { Text = "$" };
        TextSymbol selectorAt= new TextSymbol() { Text = "@" };
        TextSymbol selectorQuote = new TextSymbol() { Text = "\"" };
        TextSymbol selectorLeftSquare = new TextSymbol() { Text = "[" };
        TextSymbol selectorRightSquare = new TextSymbol() { Text = "]" };
        TextSymbol selectorLeftParenthesis= new TextSymbol() { Text = "(" };
        TextSymbol selectorRightParenthesis = new TextSymbol() { Text = ")" };

        TextSymbol entryDefinition = new TextSymbol() { Text = ":" };
        TextSymbol entrySemicolonEnd = new TextSymbol() { Text = ";" };
        TextSymbol entryNewlineEnd = new TextSymbol() { Text = "\n" };

        TextSymbol commentBegin = new TextSymbol() { Text = "/*" };
        TextSymbol commentEnd = new TextSymbol() { Text = "*/" };

        TextSymbol ignoreReturn = new TextSymbol() { Text = "\r" };
        TextSymbol newline = new TextSymbol() { Text = "\n" };
        TextSymbol space = new TextSymbol() { Text = " " };
        TextSymbol tab = new TextSymbol() { Text = "\t" };
        TextSymbol comma = new TextSymbol() { Text = "," };

        protected override CssOperationMode StartMode { get; set; } = new CssOperationMode() { Name = "Start" };

        CssOperationMode blockInnerMode = new CssOperationMode() { Name = "Block Inner Mode" };
        CssOperationMode blockEntryMode = new CssOperationMode() { Name = "Block Entry Mode" };
        CssOperationMode multiLineMode = new CssOperationMode() { Name = "Multiple Line Entry Mode" };
        CssOperationMode commentMode = new CssOperationMode() { Name = "Comment Mode" };


        public CssLanguage()
        {
            SymbolTree.Name = "CSS Symbol Tree";

            intermediateAction = new Action<CssProcessContext>(context =>
            {
                if (context.Mode == StartMode)
                {
                    context.Block.SelectorParts.Add(context.MatchValue);
                }
                if (context.Mode == blockInnerMode)
                {
                    context.BlockEntry.Property = context.MatchValue;
                }
                if (context.Mode == blockEntryMode)
                {
                    context.LineValue.Parts.Add(context.MatchValue);
                }
                if (context.Mode == multiLineMode)
                {
                    context.LineValue.Parts.Add(context.MatchValue);
                }
            });

            #region Start
            AddSymbolMode(new CssOperationMode[] { StartMode }, new TextSymbol[] { newline },
                new Action<CssProcessContext>(context =>
                {
                }));

            AddSymbolMode(new CssOperationMode[] { StartMode }, new TextSymbol[] { selectorIdStart, selectorClassStart, selectorSingleColon, selectorDoubleColon, space, tab,
                    selectorPlus, selectorRightAngle, selectorEquals, selectorCarrot, selectorTilde, selectorPipe, selectorDollarSign, selectorAt, selectorQuote,
                    selectorLeftSquare, selectorRightSquare, selectorLeftParenthesis, selectorRightParenthesis},
                new Action<CssProcessContext>(context =>
                {
                    context.Block.SelectorParts.Add(context.Match.Value.Text);
                }));

            AddSymbolMode(new CssOperationMode[] { StartMode }, new TextSymbol[] { blockScopeBegin }, new Action<CssProcessContext>(context =>
            {
                context.Document.AddBlock(context.Block);
                context.BlockEntry = new CssBlockEntry();
                context.ModeStack.Push(blockInnerMode);
            }));
            AddSymbolMode(new CssOperationMode[] { blockInnerMode }, new TextSymbol[] { blockScopeEnd }, new Action<CssProcessContext>(context =>
            {
                context.Block = new CssBlock();
                context.ModeStack.Pop();
            }));
            #endregion

            #region InnerBlock
            AddSymbolMode(new CssOperationMode[] { blockInnerMode }, new TextSymbol[] { entryDefinition }, new Action<CssProcessContext>(context =>
            {
                context.ModeStack.Push(blockEntryMode);
                context.Block.BlockEntries.Add(context.BlockEntry);
                context.LineValue = new CssLineValue();
                context.BlockEntry.LineValues.Add(context.LineValue);
            }));
            AddSymbolMode(new CssOperationMode[] { blockInnerMode }, new TextSymbol[] { newline, space, tab, entrySemicolonEnd }, new Action<CssProcessContext>(context =>
            {
                //ignore
            }));
            AddSymbolMode(new CssOperationMode[] { blockEntryMode }, new TextSymbol[] { space, tab }, new Action<CssProcessContext>(context =>
            {
                context.LineValue.Parts.Add(context.Match.Value.Text);
            }));
            //AddSymbolMode(new CssOperationMode[] { blockEntryMode }, new TextSymbol[] { comma }, new Action<CssProcessContext>(context =>
            //{
            //    context.LineValue = new CssLineValue();
            //    context.BlockEntry.LineValues.Add(context.LineValue);
            //    context.ModeStack.Push(multiLineMode);
            //}));
            AddSymbolMode(new CssOperationMode[] { blockEntryMode }, new TextSymbol[] { entrySemicolonEnd, entryNewlineEnd }, new Action<CssProcessContext>(context =>
            {
                context.BlockEntry = new CssBlockEntry();
                context.ModeStack.Pop();
            }));
            #endregion

            #region multiLineMode
            //AddSymbolMode(new CssOperationMode[] { multiLineMode }, new TextSymbol[] { newline }, new Action<CssProcessContext>(context =>
            //{
            //}));
            //AddSymbolMode(new CssOperationMode[] { multiLineMode }, new TextSymbol[] { space, tab }, new Action<CssProcessContext>(context =>
            //{
            //    context.LineValue.Parts.Add(context.Match.Value.Text);
            //}));
            //AddSymbolMode(new CssOperationMode[] { multiLineMode }, new TextSymbol[] { comma }, new Action<CssProcessContext>(context =>
            //{
            //    context.LineValue = new CssLineValue();
            //    context.BlockEntry.LineValues.Add(context.LineValue);
            //}));
            //AddSymbolMode(new CssOperationMode[] { multiLineMode }, new TextSymbol[] { blockScopeEnd }, new Action<CssProcessContext>(context =>
            //{
            //    context.BlockEntry.LineValues.Remove(context.BlockEntry.LineValues.Last());

            //    //pop multiLineMode
            //    context.ModeStack.Pop();
            //    //pop blockEntryMode
            //    context.ModeStack.Pop();
            //    //pop blockInnerMode
            //    context.ModeStack.Pop();

            //    context.Block = new CssBlock();
            //}));

            #endregion


            AddSymbolMode(new CssOperationMode[] { StartMode, blockInnerMode, blockEntryMode, multiLineMode }, new TextSymbol[] { ignoreReturn }, new Action<CssProcessContext>(context =>
            {
            }));

            #region Comment
            AddSymbolMode(new CssOperationMode[] { StartMode, blockInnerMode, blockEntryMode, multiLineMode }, new TextSymbol[] { commentBegin }, new Action<CssProcessContext>(context =>
            {
                context.ModeStack.Push(commentMode);
            }));
            AddSymbolMode(new CssOperationMode[] { commentMode }, new TextSymbol[] { commentEnd }, new Action<CssProcessContext>(context =>
            {
                context.ModeStack.Pop();
            }));
            #endregion

        }


        protected override CssProcessContext CreateContext()
        {
            return new CssProcessContext()
            {
                Document = new CssDocument(),
                Block = new CssBlock()
            };
        }

    }


}
