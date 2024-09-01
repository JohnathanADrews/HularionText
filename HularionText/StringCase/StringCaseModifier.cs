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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HularionText.StringCase
{
    /// <summary>
    /// Modifies the case of string as indicated.
    /// </summary>
    public class StringCaseModifier
    {

        private Func<string, string> casing;

        private Dictionary<StringCaseDefinition, Func<string, string>> casings = new Dictionary<StringCaseDefinition, Func<string, string>>();

        /// <summary>
        /// The case definition supplied in the constructor. This will be used when ModifyCase is called without a specified case definition.
        /// </summary>
        public StringCaseDefinition CaseDefinition { get; private set; } = StringCaseDefinition.Original;

        /// <summary>
        /// A func that appropriately modifies the case of a string.
        /// </summary>
        public Func<string, string> CaseModifier { get; private set; }

        /// <summary>
        /// A transform that appropriately modifies the case of a string.
        /// </summary>
        public ITransform<string, string> CaseTransform { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StringCaseModifier()
        {
            Initialize(StringCaseDefinition.Original);
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="caseDefinition">The case to use when ModifyCase is called without a specified case definition.</param>
        public StringCaseModifier(StringCaseDefinition caseDefinition)
        {
            Initialize(caseDefinition);
        }

        private void Initialize(StringCaseDefinition caseDefinition)
        {

            casings.Add(StringCaseDefinition.Original, x => x);
            casings.Add(StringCaseDefinition.StartLower, x => ToStartLower(x));
            casings.Add(StringCaseDefinition.StartUpper, x => ToStartUpper(x));
            casings.Add(StringCaseDefinition.AllUpper, x => String.IsNullOrWhiteSpace(x) ? x : x.ToUpper());
            casings.Add(StringCaseDefinition.AllLower, x => String.IsNullOrWhiteSpace(x) ? x : x.ToLower());

            this.CaseDefinition = caseDefinition;
            casing = casings[caseDefinition];
            CaseModifier = casing;
            CaseTransform = new TransformFunction<string, string>(casing);
        }

        private string ToStartLower(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) { return input; }
            return input.Substring(0, 1).ToLower() + input.Substring(1, input.Length - 1);
        }

        private string ToStartUpper(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) { return input; }
            return input.Substring(0, 1).ToUpper() + input.Substring(1, input.Length - 1);
        }

        /// <summary>
        /// Modifies the case using the provided case definition.
        /// </summary>
        /// <param name="value">The value to modify.</param>
        /// <param name="caseDefinition">The case definition to use.</param>
        /// <returns>The modified value.</returns>
        public string ModifyCase(string value, StringCaseDefinition caseDefinition)
        {
            return casings[caseDefinition](value);
        }

        /// <summary>
        /// Modifies the case using the case definition set in the constructor or Original if the default constructor is used.
        /// </summary>
        /// <param name="value">The value to modify.</param>
        /// <returns>The modified value.</returns>
        public string ModifyCase(string value)
        {
            return casing(value);
        }

    }
}
