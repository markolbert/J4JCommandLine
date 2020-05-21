using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;

namespace J4JSoftware.CommandLine
{
    public class SimpleHelpErrorProcessor : HelpErrorProcessor
    {
        private readonly string _keyDetailSep;

        public SimpleHelpErrorProcessor(
            IParsingConfiguration parseConfig,
            IOutputConfiguration outputConfig
        )
            : base( parseConfig, outputConfig )
        {
            _keyDetailSep = new string( ' ', OutputConfiguration.KeyDetailSeparation );
        }

        public override void Display( MappingResults result, CommandLineContext context )
        {
            base.Display(result, context);

            if( !HasErrors && !HelpRequested )
                return;

            if( !string.IsNullOrEmpty( context.Description ) )
            {
                Console.WriteLine( context.Description );
                Console.WriteLine();
            }

            if( HasErrors )
                DisplayErrors();

            if( HelpRequested || HasErrors )
                DisplayHelp();
        }

        protected void DisplayErrors()
        {
            if( Context.Errors.Count == 0 )
            {
                Console.WriteLine("Errors were encountered but not described");
                return;
            }

            foreach (var errorGroup in Context.Errors.OrderBy(e => e.Source.Key))
            {
                var keys = MergeWords( 
                    ParsingConfiguration.ConjugateKey( errorGroup.Source.Key ),
                    OutputConfiguration.KeyAreaWidth,
                    ", ");

                var errorLines = new List<string>();

                foreach (var error in errorGroup.Errors)
                {
                    if (errorLines.Count > 0)
                        errorLines.Add("\n");

                    errorLines.AddRange(
                        MergeWords(
                            error.Split( ' ', StringSplitOptions.RemoveEmptyEntries ).ToList(),
                            OutputConfiguration.DetailAreaWidth
                            , " ") );
                }

                OutputToConsole(keys, errorLines);
                
                Console.WriteLine();
            }
        }

        protected void DisplayHelp()
        {
            var sb = new StringBuilder();

            if( !string.IsNullOrEmpty( Context.ProgramName ) )
                sb.Append( $"{Context.ProgramName} " );

            sb.Append("command line options");

            switch( ParsingConfiguration.TextComparison )
            {
                case StringComparison.Ordinal:
                case StringComparison.InvariantCulture:
                case StringComparison.CurrentCulture:
                    sb.Append( " (case sensitive):" );
                    break;

                default:
                    sb.Append( ":" );
                    break;
            }

            Console.WriteLine( sb.ToString() );
            Console.WriteLine();

            foreach( var option in Context.Options
                .OrderBy( opt => opt.FirstKey )
                .Where( opt => opt.OptionType != OptionType.Null ) )
            {
                var keys = MergeWords( 
                    option.ConjugateKeys( ParsingConfiguration ),
                    OutputConfiguration.KeyAreaWidth,
                    ", ");

                var detailText = ( option.Description ?? "*** no description provided ***" )
                    .Split( ' ', StringSplitOptions.RemoveEmptyEntries )
                    .ToList();

                var detail = MergeWords( detailText, OutputConfiguration.DetailAreaWidth, " " );

                OutputToConsole( keys, detail );

                // now output the default value if one was defined
                var defaultText = option.DefaultValue == null ? null : $"default value: {option.DefaultValue}";

                if( defaultText != null )
                {
                    detailText = defaultText.Split( ' ', StringSplitOptions.RemoveEmptyEntries )
                        .ToList();

                    detail = MergeWords( detailText, OutputConfiguration.DetailAreaWidth, " " );

                    OutputToConsole( new List<string>(), detail );
                }

                Console.WriteLine();
            }
        }

        protected List<string> MergeWords( List<string> parts, int width, string wordSep )
        {
            var retVal = new List<string>();

            var line = new StringBuilder();
            var partNum = -1;
            string? part = null;

            while( partNum < parts.Count )
            {
                if( string.IsNullOrEmpty( part ) )
                {
                    partNum++;

                    if( partNum >= parts.Count )
                        break;

                    part = parts[ partNum ];
                }

                if( line.Length > 0 )
                    line.Append( wordSep );

                while( part != null )
                {
                    var available = width - line.Length - 1;

                    if (part.Length + 1 <= available)
                    {
                        line.Append(part);
                        part = null;    // added entire part, trigger move to next part
                    }
                    else
                    {
                        if (part.Length <= width)
                        {
                            // if the entire part can fit on one line, output the
                            // current line and try to fit the part in again on
                            // a new line
                            retVal.Add(line.ToString());
                            line.Clear();
                        }
                        else
                        {
                            // part is too big for a single line, so break it
                            // up (this is arbitrary but it's simple)
                            line.Append( part.Substring( 0, available ) );

                            part = part.Substring( available );
                        }
                    }
                }
            }

            // add remaining line to output if there is content in it
            if( line.Length > 0 )
                retVal.Add(line.ToString());

            return retVal;
        }

        protected virtual void OutputToConsole(List<string> keyLines, List<string> detailLines)
        {
            var numDetail = detailLines.Count;

            var maxLines = keyLines.Count;
            maxLines = maxLines > numDetail ? maxLines : numDetail;

            for (var idx = 0; idx < maxLines; idx++)
            {
                var keyArea = idx >= keyLines.Count ? string.Empty : keyLines[idx];
                keyArea = keyArea.PadRight( OutputConfiguration.KeyAreaWidth );

                var detailArea = idx < detailLines.Count ? detailLines[ idx ] : string.Empty;
                detailArea = detailArea.PadRight( OutputConfiguration.DetailAreaWidth );

                Console.WriteLine($"{keyArea}{_keyDetailSep}{detailArea}");
            }
        }
    }
}
