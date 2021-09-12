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
using J4JSoftware.Logging;
using Serilog.Events;

namespace J4JSoftware.Configuration.CommandLine
{
    public class OptionsGenerator : CustomizedEntity, IOptionsGenerator
    {
        private readonly IJ4JLogger? _logger;

        private IOptionCollection? _options;
        private StringComparison _textComparison;
        private bool _initialized;
        private CommandLineArgument? _current;

        protected OptionsGenerator(
            IJ4JLogger? logger
        )
        :base(true)
        {
            _logger = logger;
        }

        public bool IsInitialized => _initialized;

        public void Initialize( StringComparison textComparision, IOptionCollection options )
        {
            _textComparison = textComparision;
            _options = options;
            _initialized = true;
        }

        private void Validate()
        {
            if( _initialized )
                return;

            var mesg = "OptionsGenerator is uninitialized, terminating";
            _logger?.Fatal( mesg );
            throw new InvalidOperationException( mesg );
        }

        public bool Create( TokenPair tokenPair )
        {
            Validate();

            _current = new CommandLineArgument( _options!, _textComparison );
            return true;
        }

        public bool EndParsing( TokenPair tokenPair )
        {
            Validate();

            if( tokenPair.Current.Type != TokenType.EndOfInput )
            {
                LogTokenPair( tokenPair, 
                    $"{nameof( EndParsing )} called before end of command line text",
                    LogEventLevel.Error );
                return false;
            }

            if( _current == null )
            {
                _options!.UnkeyedValues.Add( tokenPair.Previous.Text );
                return true;
            }

            return AssignToOption( tokenPair );
        }

        public bool TerminateWithPrejuidice( TokenPair tokenPair )
        {
            Validate();

            _current = null;

            LogTokenPair(tokenPair, "terminated with prejudice", LogEventLevel.Error);

            return false;
        }

        public bool Commit( TokenPair tokenPair )
        {
            Validate();

            if ( _current == null )
            {
                switch( tokenPair.Current.Type )
                {
                    // if we're not yet building an entry but we received a KeyPrefix token we must be
                    // about to start building one
                    case TokenType.Separator 
                        when tokenPair.Previous != null && tokenPair.Previous.Type == TokenType.Separator:
                        return true;

                    case TokenType.KeyPrefix:
                        return Create(tokenPair);

                    default:
                        LogTokenPair(tokenPair, "invalid token sequence", LogEventLevel.Error);
                        return false;
                }
            }

            return AssignToOption(tokenPair) && Create( tokenPair );
        }

        private bool AssignToOption( TokenPair tokenPair )
        {
            if( _current == null )
                return false;

            // where we add values depends on how many are expected/required
            // by the option and what kind of option it is
            // Switch options should've been committed by this point -- so 
            // the "switch" branch should never run -- but just to be safe...
            if ( _current.Option != null )
                return _current.Option.Style == OptionStyle.Switch
                    ? CommitSwitch()
                    : CommitNonSwitch();

            _options!.UnknownKeys.Add( _current );

            LogTokenPair( tokenPair, $"unexpected key '{_current.Key}'", LogEventLevel.Error );

            return true;
        }

        // a no-op so we just consume a token but do nothing with it
        public bool ConsumeToken( TokenPair tokenPair )
        {
            Validate();

            return true;
        }

        public bool ProcessText( TokenPair tokenPair )
        {
            Validate();

            if ( _current == null )
                _options!.UnkeyedValues.Add( tokenPair.Current.Text );
            else
            {
                if( _current.Key == null )
                    _current.Key = tokenPair.Current.Text;
                else _current.Values.Add( tokenPair.Current.Text );
            }

            return true;
        }

        private bool CommitSwitch()
        {
            if( _current?.Option?.Style != OptionStyle.Switch )
            {
                _logger?.Error("Trying to commit a value to a switch value to a non-switch option");
                return false;
            }

            _current.Option.CommandLineKeyProvided = _current.Key;

            // switches never have user-specified values associated with them
            // so any values that >>appear<< to be associated with a switch
            // are unkeyed values
            _current.Option
                .Container
                .UnkeyedValues
                .AddRange( _current.Values );

            _current = null;

            return true;
        }

        private bool CommitNonSwitch()
        {
            if( _current?.Option == null || _current.Option.Style == OptionStyle.Switch )
            {
                _logger?.Error("Trying to commit a non-switch value to a switch option");
                return false;
            }

            _current.Option.CommandLineKeyProvided = _current.Key;

            _current.Option
                .AddValues( _current.Values.Take( _current.Option.MaxValues ) );

            _current.Option
                .Container
                .UnkeyedValues
                .AddRange( _current.Values.Skip( _current.Option.MaxValues ) );

            _current = null;

            return true;
        }

        protected void LogTokenPair( TokenPair tokenPair, string text, LogEventLevel level )
        {
            if( _logger == null )
                return;

            if( tokenPair.Previous == null )
                _logger.Write<string, TokenType, string>( level,
                    "{0} *undefined* => {1} ('{2}')",
                    text,
                    tokenPair.Current.Type,
                    tokenPair.Current.Text );
            else
                _logger.Write( level,
                        "{0} {1} ('{2}') => {3} ('{4}')",
                        new object[]
                        {
                            text,
                            tokenPair.Previous.Type,
                            tokenPair.Previous.Text,
                            tokenPair.Current.Type,
                            tokenPair.Current.Text
                        } );
        }
    }
}