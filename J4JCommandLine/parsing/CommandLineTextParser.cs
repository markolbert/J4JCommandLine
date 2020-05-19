using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    /// <summary>
    ///     Based on the ArgumentParser class: https://gist.github.com/shadowfox/5844284,
    ///     which is in turn based on Richard Lopes' (GriffonRL's) class at
    ///     http://www.codeproject.com/Articles/3111/C-NET-Command-Line-Arguments-Parser
    /// </summary>
    public class CommandLineTextParser : ICommandLineTextParser
    {
        private readonly IJ4JLogger? _logger;
        private readonly IParsingConfiguration _parseConfig;
        private readonly Regex _remover;
        private readonly Regex _splitter;

        public CommandLineTextParser(
            IParsingConfiguration parseConfig,
            IJ4JLogger? logger = null
        )
        {
            _parseConfig = parseConfig;
            _logger = logger;

            _logger?.SetLoggedType( GetType() );

            if( parseConfig.Prefixes.Count == 0 )
            {
                _logger?.Fatal( "No command line prefixes defined" );
                throw new ApplicationException( "No command line prefixes defined" );
            }

            if( parseConfig.TextDelimiters.Count == 0 )
            {
                _logger?.Fatal( "No command line text delimiters defined" );
                throw new ApplicationException( "No command line text delimiters defined" );
            }

            if( parseConfig.ValueEnclosers.Count == 0 )
                _logger?.Warning( "No command line value enclosers defined" );

            // create the regex remover

            var regexOptions = RegexOptions.Compiled;

            switch( _parseConfig.TextComparison )
            {
                case StringComparison.Ordinal:
                case StringComparison.CurrentCulture:
                case StringComparison.InvariantCulture:
                    regexOptions |= RegexOptions.IgnoreCase;
                    break;
            }

            _splitter = new Regex( GetRegexSplitter( parseConfig ), regexOptions );

            var rxRemover = GetRegexRemover( parseConfig );
            _remover = new Regex( @$"{rxRemover}?(.*?){rxRemover}?$", regexOptions );
        }

        public ParseResults Parse( string[] values )
        {
            var retVal = new ParseResults( _parseConfig );

            string? curKey = null;
            var parameters = new List<string>();

            // Valid parameters forms:
            // {prefix}key{encloser}({delimiter}}value{delimiter})*
            // console apps appear to split the command line based on space characters into
            // an array of arguments. An argument can be an option declaration (i.e., something
            // introduced by a special declarator, like /, - or --), a value (i.e., just plain
            // text without any declarator) or a combination (e.g., -x:abc) of a declarator and a
            // value bound together with an enclosurer text (the : in the example above).
            foreach( var value in values )
            {
                var parts = _splitter.Split( value, 3 );

                switch( parts.Length )
                {
                    // a single part means "just a value", i.e., there was no declarator
                    // present. So add the value to our list of parameters
                    case 1:
                        // if there's no option pending (i.e., being processed) something
                        // has gone wrong because we've encountered a "naked value", one
                        // not following a declared option
                        if( string.IsNullOrEmpty( curKey ) )
                            throw new ApplicationException(
                                $"Found command line value '{parts[ 0 ]}' unassociated with an option" );

                        // clean up the value before caching it
                        parameters.Add( _remover.Replace( parts[ 0 ], "$1" ) );

                        break;

                    // two parts means we found something introduced by a declarator but
                    // no value was present. So emit the result based on the previously
                    // declared tag and any captured parameters.
                    case 2:
                        // if we're currently processing a tag/option, emit it
                        if( !string.IsNullOrEmpty( curKey ) )
                            emit_option( curKey, parameters );

                        // update the key/option we're processing and reset the parameters
                        // collection
                        curKey = parts[ 1 ];
                        parameters.Clear();

                        break;

                    // three parts means we found a combined declarator/value. So emit the
                    // result based on the previously declared tag and any captured parameters.
                    case 3:
                        // if we're currently processing a tag/option, emit it
                        if( !string.IsNullOrEmpty( curKey ) )
                            emit_option( curKey, parameters );

                        // update the key/option we're processing and reset the parameters
                        // collection
                        curKey = parts[ 1 ];
                        parameters.Clear();

                        // Remove possible enclosing characters from the value portion and store
                        // it in the parameters collection
                        parameters.Add( _remover.Replace( parts[ 2 ], "$1" ) );

                        // emit the option, since combined declarator/values are atomic (i.e., they
                        // can't contain multiple parameters) and indicate we're no longer
                        // currently processing an option
                        emit_option( curKey, parameters );

                        // since combined declarator/values are atomic (i.e., they can't contain
                        // multiple parameters) indicate we're no longer processing an option
                        curKey = null;
                        parameters.Clear();

                        break;
                }
            }

            // In case an option is still waiting to be emitted
            if( !string.IsNullOrEmpty( curKey ) )
                emit_option( curKey, parameters );

            return retVal;

            void emit_option( string key, List<string> keyValues )
            {
                // retVal will never be null when this private function is called
                if( retVal == null )
                    throw new ApplicationException( $"{nameof(ParseResults)} is undefined." );

                // an empty keyValues list means this was a switch option so add
                // the value "true" to keyValues
                if( keyValues.Count == 0 )
                    keyValues.Add( "true" );

                // if retVal already contains an entry for key merge the current
                // parameter list into the exiting parameter list
                if( retVal.Contains( key ) )
                {
                    retVal[ key ].Parameters.AddRange( keyValues );
                }
                else
                {
                    var entry = new ParseResult { Key = key };
                    entry.Parameters.AddRange( keyValues );

                    retVal.Add( entry );
                }
            }
        }

        private string GetRegexRemover( IParsingConfiguration parseConfig )
        {
            var retVal = new StringBuilder( "[" );

            for( var idx = 0; idx < parseConfig.TextDelimiters.Count; idx++ )
            {
                if( idx > 0 )
                    retVal.Append( "|" );

                retVal.Append( parseConfig.TextDelimiters[ idx ] );
            }

            retVal.Append( "]" );

            return retVal.ToString();
        }

        private string GetRegexSplitter( IParsingConfiguration parseConfig )
        {
            var retVal = new StringBuilder();

            foreach( var prefix in parseConfig.Prefixes )
            {
                if( retVal.Length > 0 )
                    retVal.Append( "|" );

                retVal.Append( $"^{prefix}" );
            }

            foreach( var encloser in parseConfig.ValueEnclosers )
            {
                if( retVal.Length > 0 )
                    retVal.Append( "|" );

                retVal.Append( $"{encloser}" );
            }

            return retVal.ToString();
        }
    }
}