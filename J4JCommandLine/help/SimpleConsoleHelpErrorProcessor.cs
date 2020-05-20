using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;

namespace J4JSoftware.CommandLine
{
    public class SimpleConsoleHelpErrorProcessor : HelpErrorProcessor
    {
        private readonly string _keyDetailSep;

        public SimpleConsoleHelpErrorProcessor(
            IParsingConfiguration parseConfig,
            IOutputConfiguration outputConfig
        )
            : base( parseConfig, outputConfig )
        {
            _keyDetailSep = new string( ' ', OutputConfiguration.KeyDetailSeparation );
        }

        public override void Display( IOptionCollection options, CommandLineErrors errors, string? headerMesg = null )
        {
            if( !string.IsNullOrEmpty( headerMesg ) )
                Console.WriteLine( headerMesg );

            if( errors.Count > 0 )
                DisplayErrors( errors );

            DisplayHelp( options );
        }

        protected void DisplayErrors( CommandLineErrors errors )
        {
            foreach (var errorGroup in errors.OrderBy(e => e.Source.Key))
            {
                var keys = BreakLineAtSpaces(errorGroup.Source.Key, OutputConfiguration.KeyAreaWidth);

                var errorLines = new List<string>();

                foreach (var error in errorGroup.Errors)
                {
                    if (errorLines.Count > 0)
                        errorLines.Add("\n");

                    errorLines.AddRange(BreakLineAtSpaces(error, OutputConfiguration.DetailAreaWidth));
                }

                OutputToConsole(keys, errorLines);
            }
        }

        protected void DisplayHelp( IOptionCollection options )
        {
            foreach( var option in options
                .OrderBy( opt => opt.FirstKey )
                .Where( opt => opt.OptionType != OptionType.Null ) )
            {
                var keys = BreakLineAtSpaces( 
                    option.FormatKeys( ParsingConfiguration ),
                    OutputConfiguration.KeyAreaWidth );

                var detail = BreakLineAtSpaces( 
                    option.Description ?? "*** no description provided ***",
                    OutputConfiguration.DetailAreaWidth );

                if( option.DefaultValue != null )
                    detail.Add( $"default value: {option.DefaultValue}" );

                OutputToConsole( keys, detail );
            }
        }

        protected List<string> BreakLineAtSpaces( string text, int width )
        {
            var retVal = new List<string>();

            var parts = text.Split( " ", StringSplitOptions.RemoveEmptyEntries );
            var line = new StringBuilder();
            var partNum = -1;
            string? part = null;

            while( partNum < parts.Length )
            {
                if( string.IsNullOrEmpty( part ) )
                {
                    partNum++;
                    if( partNum >= parts.Length )
                        break;

                    part = parts[ partNum ];
                }

                if (line.Length > 0)
                    line.Append(" ");

                var available = width - line.Length - 1;

                if( part.Length + 1 <= available )
                {
                    line.Append( part );

                    part = null;    // trigger move to next part
                }
                else
                {
                    line.Append( $" {part.Substring( 0, available )}" );

                    retVal.Add(line.ToString());

                    line.Clear();

                    part = part.Substring( available );
                }
            }

            if( line.Length < width )
                line.Append( new string( ' ', width - line.Length ) );

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

            Console.WriteLine();
        }
    }
}
