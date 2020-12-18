using System.Collections.Generic;
using J4JSoftware.CommandLine;
#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public class OptionConfig
    {
        public string ContextKey { get; set; }
        public string CommandLineKey { get; set; }
        public OptionStyle Style { get; set; }
        public bool Required { get; set; }
        public bool ParsingWillFail { get; set; }
        public bool ValuesSatisfied { get; set; }
        public string? CorrectText { get; set; }
        public List<string> CorrectTextArray { get; set; }
        public Option Option { get; private set; }

        public void CreateOption( Options options )
        {
            var retVal = options.Add( ContextKey )
                .AddCommandLineKey( CommandLineKey )
                .SetStyle( Style );

            if( Required ) retVal.IsRequired();
            else retVal.IsOptional();

            Option = retVal;
        }
    }
}