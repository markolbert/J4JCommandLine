#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// OptionsGenerator.cs
//
// This file is part of JumpForJoy Software's J4JCommandLine.
// 
// J4JCommandLine is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JCommandLine is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JCommandLine. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class OptionsGenerator : IOptionsGenerator
{
    public static OptionsGenerator GetWindowsDefault( ILoggerFactory? loggerFactory = null ) =>
        new OptionsGenerator( OptionCollection.GetWindowsDefault( loggerFactory ),
                              StringComparison.OrdinalIgnoreCase,
                              loggerFactory );

    public static OptionsGenerator GetLinuxDefault( ILoggerFactory? loggerFactory = null ) =>
        new OptionsGenerator( OptionCollection.GetWindowsDefault( loggerFactory ),
                              StringComparison.Ordinal,
                              loggerFactory );

    private readonly OptionCollection? _options;
    private readonly StringComparison _textComparison;
    private readonly ILogger? _logger;

    private CommandLineArgument? _current;

    public OptionsGenerator( 
        OptionCollection options,
        StringComparison textComparison,
        ILoggerFactory? loggerFactory = null 
        )
    {
        _options = options;
        _textComparison = textComparison;
        _logger = loggerFactory?.CreateLogger<OptionsGenerator>();
    }

    public bool Create( TokenPair tokenPair )
    {
        _current = new CommandLineArgument( _options!, _textComparison );
        return true;
    }

    public bool EndParsing( TokenPair tokenPair )
    {
        if( tokenPair.Current.Type != LexicalType.EndOfInput )
        {
            LogTokenPair( tokenPair,
                          $"{nameof( EndParsing )} called before end of command line text",
                          LogLevel.Error );
            return false;
        }

        if( _current == null )
        {
            _options!.SpuriousValues.Add( tokenPair.Previous.Text );
            return true;
        }

        return AssignToOption( tokenPair );
    }

    public bool TerminateWithPrejudice( TokenPair tokenPair )
    {
        _current = null;

        LogTokenPair( tokenPair, "terminated with prejudice", LogLevel.Error );

        return false;
    }

    public bool Commit( TokenPair tokenPair )
    {
        if ( _current == null )
        {
            switch( tokenPair.Current.Type )
            {
                // if we're not yet building an entry but we received a KeyPrefix token we must be
                // about to start building one
                case LexicalType.Separator
                    when tokenPair.Previous.Type == LexicalType.Separator:
                    return true;

                case LexicalType.KeyPrefix:
                    return Create( tokenPair );

                default:
                    LogTokenPair( tokenPair, "invalid token sequence", LogLevel.Error );
                    return false;
            }
        }

        return AssignToOption( tokenPair ) && Create( tokenPair );
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

        LogTokenPair( tokenPair, $"unexpected key '{_current.Key}'", LogLevel.Error );

        return true;
    }

    // a no-op so we just consume a token but do nothing with it
    public bool ConsumeToken( TokenPair tokenPair ) => true;

    public bool ProcessText( TokenPair tokenPair )
    {
        if ( _current == null )
            _options!.SpuriousValues.Add( tokenPair.Current.Text );
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
            _logger?.LogError( "Trying to commit a value to a switch value to a non-switch option" );
            return false;
        }

        _current.Option.CommandLineKeyProvided = _current.Key;

        // switches never have user-specified values associated with them
        // so any values that >>appear<< to be associated with a switch
        // are spurious values
        _current.Option
                .Collection
                .SpuriousValues
                .AddRange( _current.Values );

        _current = null;

        return true;
    }

    private bool CommitNonSwitch()
    {
        if( _current?.Option == null || _current.Option.Style == OptionStyle.Switch )
        {
            _logger?.LogError( "Trying to commit a non-switch value to a switch option" );
            return false;
        }

        // check for invalid Enum values because they'll blow up J4JCommandLineProvider (because
        // the actual value conversion takes place deep inside the IConfiguration subsystem, which
        // doesn't check for invalid values, or fail gracefully; it just throws an exception
        if( _current.Option.PropertyType.IsEnum
        && !ValidEnumValue( _current.Values.Take( _current.Option.MaxValues ), _current.Option.PropertyType ) )
            return false;

        _current.Option.CommandLineKeyProvided = _current.Key;

        _current.Option
                .AddValues( _current.Values.Take( _current.Option.MaxValues ) );

        _current.Option
                .Collection
                .SpuriousValues
                .AddRange( _current.Values.Skip( _current.Option.MaxValues ) );

        _current = null;

        return true;
    }

    private bool ValidEnumValue( IEnumerable<string> values, Type propertyType )
    {
        if( !propertyType.IsEnum )
            return true;

        var validValues = Enum.GetNames( propertyType );

        foreach( var value in values )
        {
            if( validValues.Any( x => x.Equals( value, _textComparison ) ) )
                continue;

            _logger?.LogError( "Invalid {0} option value '{1}'", propertyType.Name, value );
            return false;
        }

        return true;
    }

    protected void LogTokenPair( TokenPair tokenPair, string text, LogLevel level )
    {
        if( _logger == null )
            return;

        _logger.Log( level,
                     "{0} *undefined* => {1} ('{2}')",
                     text,
                     tokenPair.Current.Type,
                     tokenPair.Current.Text );

        //else
        //    _logger.Write( level,
        //            "{0} {1} ('{2}') => {3} ('{4}')",
        //            new object[]
        //            {
        //                text,
        //                tokenPair.Previous.Type,
        //                tokenPair.Previous.Text,
        //                tokenPair.Current.Type,
        //                tokenPair.Current.Text
        //            } );
    }
}