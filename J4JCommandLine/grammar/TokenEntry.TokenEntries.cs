﻿using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class TokenEntry
    {
        public class TokenEntries
        {
            private readonly IJ4JLogger? _logger;

            public TokenEntries(
                IOptionCollection options,
                IJ4JLogger? logger
            )
            {
                Options = options;
                _logger = logger;
            }

            public IOptionCollection Options { get; }
            public TokenEntry? Current { get; private set; }

            public bool Create( Token prevToken, Token curToken, params string[] args )
            {
                Current = new TokenEntry( this );

                return true;
            }

            public bool TerminateWithPrejuidice( Token prevToken, Token curToken, params string[] args )
            {
                _logger?.Error( "{0} ('{1}') => {2} ('{3}'): {4}",
                    new object[] { curToken.Type, curToken.Text, prevToken.Type, prevToken.Text, "invalid token sequence" } );

                Current = null;

                return false;
            }

            public bool Commit( Token prevToken, Token curToken, params string[] args )
            {
                if( Current == null )
                {
                    // if we're not yet building an entry but we received a KeyPrefix token we must be
                    // about to start building one
                    if( curToken.Type == TokenType.KeyPrefix )
                        return Create( prevToken, curToken );

                    _logger?.Error("{0} ('{1}') => {2} ('{3}'): {4}",
                        new object[] { curToken.Type, curToken.Text, prevToken.Type, prevToken.Text, "undefined TokenEntry" });

                    return false;
                }

                if( Current.Option == null )
                {
                    _logger?.Error( "{0} ('{1}') => {2} ('{3}'): {4}",
                        new object[]
                        {
                            curToken.Type, curToken.Text, prevToken.Type, prevToken.Text,
                            $"unexpected key '{Current.Key}'"
                        } );

                    Options.UnknownKeys.Add( Current );

                    // create a new TokenEntry
                    Create( prevToken, curToken, args );

                    return false;
                }

                // where we add values depends on how many are expected/required
                // by the option and what kind of option it is
                // Switch options should've been committed by this point -- so 
                // the "switch" branch should never run -- but just to be safe...
                var retVal = Current.Option.Style == OptionStyle.Switch
                    ? CommitSwitch()
                    : CommitNonSwitch();

                return retVal && Create( prevToken, curToken, args );
            }

            // a no-op so we just consume a token but do nothing with it
            public bool ConsumeToken( Token prevToken, Token curToken, params string[] args )
            {
                return true;
            }

            public bool ProcessText( Token prevToken, Token curToken, params string[] args )
            {
                if( args.Length != 1 )
                {
                    _logger?.Error("{0} ('{1}') => {2} ('{3}'): {4}",
                        new object[]
                        {
                            curToken.Type, curToken.Text, prevToken.Type, prevToken.Text,
                            $"invalid number of text arguments '{args.Length}'"
                        });

                    Current = null;

                    return false;
                }

                if( Current == null )
                {
                    Options.UnkeyedValues.Add( args[ 0 ] );
                }
                else
                {
                    if( Current.Key == null )
                        Current.Key = args[ 0 ];
                    else Current.Values.Add( args[ 0 ] );
                }

                return true;
            }

            private bool CommitSwitch()
            {
                if( Current?.Option?.Style != OptionStyle.Switch )
                    return false;

                Current.Option.CommandLineKeyProvided = Current.Key;

                // switches never have user-specified values associated with them
                // so any values that >>appear<< to be associated with a switch
                // are unkeyed values
                Current.Option
                    .Container
                    .UnkeyedValues
                    .AddRange( Current.Values );

                Current = null;

                return true;
            }

            private bool CommitNonSwitch()
            {
                if( Current?.Option == null || Current.Option.Style == OptionStyle.Switch )
                    return false;

                Current.Option.CommandLineKeyProvided = Current.Key;

                Current.Option
                    .AddValues( Current.Values.Take( Current.Option.MaxValues ) );

                Current.Option
                    .Container
                    .UnkeyedValues
                    .AddRange( Current.Values.Skip( Current.Option.MaxValues ) );

                Current = null;

                return true;
            }
        }
    }
}