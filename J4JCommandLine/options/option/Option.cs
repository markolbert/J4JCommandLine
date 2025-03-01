#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Option.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class Option<TContainer, TProp> : IOptionInternal
{
    private readonly List<string> _cmdLineKeys = [];
    private readonly ITextToValue _converter;
    private readonly List<string> _values = [];
    private readonly ILogger? _logger = BuildTimeLoggerFactory.Default.Create<Option<TContainer, TProp>>();

    internal Option(
        OptionCollection container,
        string contextPath,
        ITextToValue converter
    )
    {
        Collection = container;
        ContextPath = contextPath;
        _converter = converter;
    }

    public bool IsInitialized => !string.IsNullOrEmpty( ContextPath ) && _cmdLineKeys.Count > 0;
    public OptionCollection Collection { get; }
    public Type ContainingType => typeof( TContainer );
    public Type PropertyType => typeof( TProp );

    public string? ContextPath { get; }

    public ReadOnlyCollection<string> Keys => _cmdLineKeys.AsReadOnly();

    public Option<TContainer, TProp> AddCommandLineKey( string cmdLineKey )
    {
        if( !Collection.CommandLineKeyInUse( cmdLineKey ) )
            _cmdLineKeys.Add( cmdLineKey );

        return this;
    }

    public Option<TContainer, TProp> AddCommandLineKeys( IEnumerable<string> cmdLineKeys )
    {
        foreach( var cmdLineKey in cmdLineKeys )
        {
            AddCommandLineKey( cmdLineKey );
        }

        return this;
    }

    public string? CommandLineKeyProvided { get; set; }

    public OptionStyle Style { get; private set; } = OptionStyle.Undefined;

    public ReadOnlyCollection<string> Values => _values.AsReadOnly();

    void IOptionInternal.ClearValues()
    {
        _values.Clear();
    }

    void IOptionInternal.AddValue( string value )
    {
        _values.Add( value );
    }

    void IOptionInternal.AddValues( IEnumerable<string> values )
    {
        _values.AddRange( values );
    }

    public int MaxValues
    {
        get
        {
            var retVal = Style switch
            {
                OptionStyle.Collection => int.MaxValue,
                OptionStyle.SingleValued => 1,
                OptionStyle.ConcatenatedSingleValue => int.MaxValue,
                OptionStyle.Switch => 0,
                _ => -1
            };

            if( retVal < 0 )
                _logger?.UnsupportedOptionStyle( Style.ToString() );

            return retVal < 0 ? 0 : retVal;
        }
    }

    public int NumValuesAllocated => _values.Count;

    public bool ValuesSatisfied
    {
        get
        {
            if( string.IsNullOrEmpty( CommandLineKeyProvided ) )
                return false;

            var numValuesAlloc = _values.Count;

            switch( Style )
            {
                case OptionStyle.Switch:
                    return numValuesAlloc == 0;

                case OptionStyle.SingleValued:
                    return numValuesAlloc == 1;

                case OptionStyle.Collection:
                case OptionStyle.ConcatenatedSingleValue:
                    return numValuesAlloc > 0;

                default:
                    _logger?.UnsupportedOptionStyle( Style.ToString() );
                    return false;
            }
        }
    }

    public bool GetValue( out object? result )
    {
        result = default( TProp );

        if( !ValuesSatisfied )
            return false;

        if( Style != OptionStyle.Switch )
            return _converter != null && _converter.Convert( typeof( TProp ), _values, out result );

        result = !string.IsNullOrEmpty( CommandLineKeyProvided );

        return true;
    }

    public bool Required { get; private set; }

    public Option<TContainer, TProp> IsRequired()
    {
        Required = true;
        return this;
    }

    public Option<TContainer, TProp> IsOptional()
    {
        Required = false;
        return this;
    }

    public string? Description { get; private set; }

    public Option<TContainer, TProp> SetStyle( OptionStyle style )
    {
        Style = style;
        return this;
    }

    public Option<TContainer, TProp> SetDescription( string description )
    {
        Description = description;
        return this;
    }

    public TProp? DefaultValue { get; private set; }

    public Option<TContainer, TProp> SetDefaultValue( TProp? value )
    {
        DefaultValue = value;
        return this;
    }

    string? IOption.GetDefaultValue() => DefaultValue?.ToString();
}
