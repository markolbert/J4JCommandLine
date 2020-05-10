using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    /// <summary>
    /// Based on the ArgumentParser class: https://gist.github.com/shadowfox/5844284,
    /// which is in turn based on Richard Lopes' (GriffonRL's) class at
    /// http://www.codeproject.com/Articles/3111/C-NET-Command-Line-Arguments-Parser
    /// </summary>
    public class CommandLineTextParser : ICommandLineTextParser
    {
        private readonly IParsingConfiguration _parseConfig;
        private readonly IJ4JLogger? _logger;
        private readonly Regex _splitter;
        private readonly Regex _remover;

        public CommandLineTextParser(
            IParsingConfiguration parseConfig,
            IJ4JLogger? logger = null
            )
        {
            _parseConfig = parseConfig;
            _logger = logger;

            _logger?.SetLoggedType( this.GetType() );

            if (parseConfig.Prefixes.Count == 0)
                _logger?.Warning("No command line prefixes defined");

            if (parseConfig.TextDelimiters.Count == 0)
                _logger?.Warning("No command line text delimiters defined");

            if (parseConfig.ValueEnclosers.Count == 0)
                _logger?.Warning("No command line value enclosers defined");

            // create the regex splitter
            var sbSplitter = new StringBuilder();

            foreach( var prefix in parseConfig.Prefixes )
            {
                if( sbSplitter.Length > 0 )
                    sbSplitter.Append( "|" );

                sbSplitter.Append( $"^{prefix}" );
            }

            foreach( var encloser in parseConfig.ValueEnclosers )
            {
                if (sbSplitter.Length > 0)
                    sbSplitter.Append("|");

                sbSplitter.Append($"{encloser}");
            }

            // create the regex remover
            var sbRemover = new StringBuilder("^[");

            foreach (var delimiter in parseConfig.TextDelimiters)
            {
                if (sbRemover.Length > 0)
                    sbRemover.Append("|");

                sbRemover.Append(delimiter);
            }

            sbRemover.Append( "]" );

            var regexOptions = RegexOptions.Compiled;

            switch( _parseConfig.TextComparison )
            {
                case StringComparison.Ordinal:
                case StringComparison.CurrentCulture:
                case StringComparison.InvariantCulture:
                    regexOptions |= RegexOptions.IgnoreCase;
                    break;
            }

            _splitter = new Regex( sbSplitter.ToString(), regexOptions );
            _remover = new Regex( @$"{sbRemover.ToString()}?(.*?){sbRemover.ToString()}?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled );
        }

        public Dictionary<string, List<string>> Parse( string[] args )
        {
            var retVal = new Dictionary<string, List<string>>();

            var curTag = string.Empty;
            var prevTag = string.Empty;

            // Valid parameters forms:
            // {prefix}param{encloser}({delimiter}}value{delimiter})
            foreach( var arg in args )
            {
                var parts = _splitter.Split( arg, 3 );

                switch( parts.Length )
                {
                    // Found a value (for the last tag found (space separator))
                    case 1:
                        if( !string.Equals(curTag, prevTag, _parseConfig.TextComparison) )
                        {
                            if( !retVal.ContainsKey( curTag ) )
                            {
                                create_entry(curTag, _remover.Replace(parts[0], "$1"));
                            }

                            prevTag = curTag;
                        }
                        else
                        {
                            retVal[ curTag ].Add( parts[ 0 ] );

                            _logger?.Verbose<string>( "Added value '{0}' to parsed tag", parts[ 0 ] );
                        }

                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. With no value, set it to true.
                        if (!string.Equals(curTag, prevTag, _parseConfig.TextComparison))
                        {
                            if( !retVal.ContainsKey( curTag ) )
                            {
                                create_entry(curTag, "true");
                            }
                        }

                        curTag = parts[ 1 ];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. With no value, set it to true.
                        if (!string.Equals(curTag, prevTag, _parseConfig.TextComparison))
                        {
                            {
                                create_entry(curTag, "true");
                            }
                        }

                        curTag = parts[ 1 ];

                        // Remove possible enclosing characters (",')
                        if( !retVal.ContainsKey( curTag ) )
                        {
                            create_entry( curTag, _remover.Replace( parts[ 2 ], "$1" ) );
                        }

                        prevTag = curTag;

                        break;
                }
            }

            // In case a parameter is still waiting
            if( string.Equals( curTag, prevTag, _parseConfig.TextComparison ) ) 
                return retVal;
            
            if ( !retVal.ContainsKey( curTag ) )
                create_entry(curTag, "true");

            return retVal;

            void create_entry( string tag, string text )
            {
                var values = new List<string>();
                values.Add(text);

                retVal?.Add(tag, values);

                _logger?.Verbose<string, string>( "Created parsed tag '{0}' and added value '{1}' to it", tag, text );
            }
        }
    }
}
