using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    // A simple, functional proof-of-concept IHelpErrorProcessor. Outputs error and help information
    // organized by command line key.
    public class SimpleHelpErrorProcessor : HelpErrorProcessor
    {
        private readonly string _keyDetailSep;
        private readonly OutputConfiguration _outConfig;
        private readonly List<string> _lines = new List<string>();

        public SimpleHelpErrorProcessor(
            IParsingConfiguration parseConfig,
            OutputConfiguration outputConfig
        )
            : base( parseConfig )
        {
            _outConfig = outputConfig;
            _keyDetailSep = new string( ' ', _outConfig.KeyDetailSeparation );
        }

        protected override void Initialize()
        {
            base.Initialize();

            _lines.Clear();
        }

        protected override void CreateHeaderSection()
        {
            if( !string.IsNullOrEmpty(ParsingConfiguration.ProgramName))
                _lines.Add(ParsingConfiguration.ProgramName  );

            if( !string.IsNullOrEmpty(ParsingConfiguration.Description))
                _lines.Add(ParsingConfiguration.Description );

            _lines.Add( string.Empty );
        }

        protected override void CreateErrorSection()
        {
            if( BindingTarget.Errors.Count == 0 )
            {
                _lines.Add("Errors were encountered but not described");
                return;
            }

            // errors are displayed organized by keys
            foreach (var errorGroup in BindingTarget.Errors.OrderBy(e => e.Source.Key))
            {
                var keys = MergeWords( 
                    ParsingConfiguration.ConjugateKey( errorGroup.Source.Key ),
                    _outConfig.KeyAreaWidth,
                    ", ");

                var errorLines = new List<string>();

                foreach (var error in errorGroup.Errors)
                {
                    if (errorLines.Count > 0)
                        errorLines.Add("\n");

                    errorLines.AddRange(
                        MergeWords(
                            error.Split( ' ', StringSplitOptions.RemoveEmptyEntries ).ToList(),
                            _outConfig.DetailAreaWidth
                            , " ") );
                }

                OutputToConsole(keys, errorLines);

                _lines.Add(string.Empty  );
            }
        }

        // help is displayed organized by key
        protected override void CreateHelpSection()
        {
            var sb = new StringBuilder();

            if( !string.IsNullOrEmpty( ParsingConfiguration.ProgramName ) )
                sb.Append( $"{ParsingConfiguration.ProgramName} " );

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

            _lines.Add(sb.ToString() );
            _lines.Add(string.Empty  );

            foreach( var option in BindingTarget.Options
                .OrderBy( opt => opt.FirstKey )
                .Where( opt => opt.OptionType != OptionType.Null ) )
            {
                var keys = MergeWords( 
                    option.ConjugateKeys( ParsingConfiguration ),
                    _outConfig.KeyAreaWidth,
                    ", ");

                var detailText = ( option.Description ?? "*** no description provided ***" )
                    .Split( ' ', StringSplitOptions.RemoveEmptyEntries )
                    .ToList();

                var detail = MergeWords( detailText, _outConfig.DetailAreaWidth, " " );

                OutputToConsole( keys, detail );

                // now output the default value if one was defined
                var defaultText = option.DefaultValue == null ? null : $"default value: {option.DefaultValue}";

                if( defaultText != null )
                {
                    detailText = defaultText.Split( ' ', StringSplitOptions.RemoveEmptyEntries )
                        .ToList();

                    detail = MergeWords( detailText, _outConfig.DetailAreaWidth, " " );

                    OutputToConsole( new List<string>(), detail );
                }

                _lines.Add(string.Empty  );
            }
        }

        protected override void DisplayOutput()
        {
            foreach( var line in _lines )
            {
                Console.WriteLine( line );
            }
        }

        // takes a list of words and merges them together using the specified wordSep (e.g.,
        // a space, a ", ") such that they fit within the specified width
        private List<string> MergeWords( List<string> words, int width, string wordSep )
        {
            var retVal = new List<string>();

            var line = new StringBuilder();
            var partNum = -1;
            string? part = null;

            while( partNum < words.Count )
            {
                if( string.IsNullOrEmpty( part ) )
                {
                    partNum++;

                    if( partNum >= words.Count )
                        break;

                    part = words[ partNum ];
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

        // outputs a block of key-focused information, either help or errors, ensuring that
        // blank lines are added when necessary to either block to make both blocks take up
        // the same number of lines
        private void OutputToConsole(List<string> keyLines, List<string> detailLines)
        {
            var numDetail = detailLines.Count;

            var maxLines = keyLines.Count;
            maxLines = maxLines > numDetail ? maxLines : numDetail;

            for (var idx = 0; idx < maxLines; idx++)
            {
                var keyArea = idx >= keyLines.Count ? string.Empty : keyLines[idx];
                keyArea = keyArea.PadRight( _outConfig.KeyAreaWidth );

                var detailArea = idx < detailLines.Count ? detailLines[ idx ] : string.Empty;
                detailArea = detailArea.PadRight( _outConfig.DetailAreaWidth );

                _lines.Add($"{keyArea}{_keyDetailSep}{detailArea}");
            }
        }
    }
}
