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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace J4JSoftware.Configuration.CommandLine;

public class Option<TContainer, TProp> : IOption, IOptionInternal
{
    private readonly List<string> _cmdLineKeys = new();
    private readonly ITextToValue _converter;
    private readonly List<string> _values = new();

    internal Option( OptionCollection container,
        string contextPath,
        ITextToValue converter )
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
        foreach( var cmdLineKey in cmdLineKeys ) AddCommandLineKey( cmdLineKey );

        return this;
    }

    public string? CommandLineKeyProvided { get; set; }

    public OptionStyle Style { get; private set; } = OptionStyle.Undefined;

    public IOption SetStyle( OptionStyle style )
    {
        Style = style;
        return this;
    }

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

    public int MaxValues =>
        Style switch
        {
            OptionStyle.Collection => int.MaxValue,
            OptionStyle.SingleValued => 1,
            OptionStyle.ConcatenatedSingleValue => int.MaxValue,
            OptionStyle.Switch => 0,
            _ => throw new InvalidEnumArgumentException( $"Unsupported OptionStyle '{Style}'" )
        };

    public int NumValuesAllocated => _values.Count;

    public bool ValuesSatisfied
    {
        get
        {
            if( string.IsNullOrEmpty( CommandLineKeyProvided ) )
                return false;

            var numValuesAlloc = _values.Count;

            return Style switch
            {
                OptionStyle.Switch => numValuesAlloc == 0,
                OptionStyle.SingleValued => numValuesAlloc == 1,
                OptionStyle.Collection => numValuesAlloc > 0,
                OptionStyle.ConcatenatedSingleValue => numValuesAlloc > 0,
                _ => throw new InvalidEnumArgumentException( $"Unsupported OptionStyle '{Style}'" )
            };
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