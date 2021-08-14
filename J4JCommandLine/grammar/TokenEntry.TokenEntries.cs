#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class TokenEntry
    {
        public class TokenEntries
        {
            private readonly IJ4JLogger? _logger;

            public TokenEntries(
                StringComparison textComparison,
                IOptionCollection options,
                IJ4JLogger? logger
            )
            {
                TextComparison = textComparison;
                Options = options;
                _logger = logger;
            }

            public IOptionCollection Options { get; }
            public StringComparison TextComparison { get; }
            public TokenEntry? Current { get; private set; }

            public bool Create( Token prevToken, Token curToken, params string[] args )
            {
                Current = new TokenEntry( this );

                return true;
            }

            public bool TerminateWithPrejuidice( Token prevToken, Token curToken, params string[] args )
            {
                _logger?.Error( "{0} ('{1}') => {2} ('{3}'): {4}",
                    new object[]
                        { curToken.Type, curToken.Text, prevToken.Type, prevToken.Text, "invalid token sequence" } );

                Current = null;

                return false;
            }

            public bool Commit( Token prevToken, Token curToken, params string[] args )
            {
                if( Current == null )
                {
                    switch( curToken.Type )
                    {
                        // if we're not yet building an entry but we received a KeyPrefix token we must be
                        // about to start building one
                        case TokenType.Separator when prevToken.Type == TokenType.Separator:
                            return true;

                        case TokenType.KeyPrefix:
                            return Create( prevToken, curToken );

                        default:
                            _logger?.Error( "{0} ('{1}') => {2} ('{3}'): {4}",
                                new object[]
                                    { curToken.Type, curToken.Text, prevToken.Type, prevToken.Text, "undefined TokenEntry" } );

                            return false;
                    }
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
                    _logger?.Error( "{0} ('{1}') => {2} ('{3}'): {4}",
                        new object[]
                        {
                            curToken.Type, curToken.Text, prevToken.Type, prevToken.Text,
                            $"invalid number of text arguments '{args.Length}'"
                        } );

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