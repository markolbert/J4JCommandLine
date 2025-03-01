#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TextConverters.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class TextConverters : ITextConverters
{
    private readonly Dictionary<Type, ITextToValue> _converters = new();
    private readonly BuiltInConverters _builtInConv;
    private readonly List<BuiltInConverter>? _builtInTargets;
    private readonly ILogger? _logger;

    private record BuiltInConverter( Type ReturnType, MethodInfo MethodInfo );

    private static List<ITextToValue> GetBuiltInConverters()
    {
        var retVal = new List<ITextToValue>();

        foreach( var builtInConverter in GetBuiltInTargetTypes() )
        {
            var builtInType = typeof( BuiltInTextToValue<> ).MakeGenericType( builtInConverter.ReturnType );

            retVal.Add( (ITextToValue) Activator.CreateInstance( builtInType, builtInConverter.MethodInfo )! );
        }

        return retVal;
    }

    private static List<BuiltInConverter> GetBuiltInTargetTypes() =>
        typeof( Convert )
           .GetMethods( BindingFlags.Static | BindingFlags.Public )
           .Where( m =>
            {
                var parameters = m.GetParameters();

                return parameters.Length == 1
                 && !typeof( string ).IsAssignableFrom( parameters[ 0 ].ParameterType );
            } )
           .Select( x => new BuiltInConverter( x.ReturnType, x ) )
           .ToList();

    public TextConverters(
        BuiltInConverters builtInConv = BuiltInConverters.AddDynamically,
        params ITextToValue[] converters
    )
    {
        _logger = BuildTimeLoggerFactory.Default.Create<TextConverters>();

        // add the text to text "converter"
        AddConverter( new TextToTextConverter(), true );

        AddConverters( converters );
        _builtInConv = builtInConv;

        switch( builtInConv )
        {
            case BuiltInConverters.AddAtInitialization:
                AddConverters( GetBuiltInConverters() );
                break;

            case BuiltInConverters.AddDynamically:
                _builtInTargets = GetBuiltInTargetTypes();
                break;
        }
    }

    public IEnumerable<Type> Keys => _converters.Keys;
    public IEnumerable<ITextToValue> Values => _converters.Values;
    public int Count => _converters.Count;

    public bool ContainsKey( Type key ) => _converters.ContainsKey( key );

    public bool AddConverter( ITextToValue converter, bool replaceExisting = false )
    {
        if( _converters.TryAdd( converter.TargetType, converter ) )
            return true;

        if( !replaceExisting )
        {
            _logger?.DuplicateConverter( converter.TargetType.Name );
            return false;
        }

        _converters[ converter.TargetType ] = converter;

        return true;
    }

    public bool AddConverters( IEnumerable<ITextToValue> converters, bool replaceExisting = false )
    {
        var retVal = true;

        foreach( var converter in converters )
        {
            retVal &= AddConverter( converter, replaceExisting );
        }

        return retVal;
    }

    public bool CanConvert( Type toCheck )
    {
        // we can convert any type for which we have a converter, plus lists and arrays of those types
        if( toCheck.IsArray )
        {
            var elementType = toCheck.GetElementType();
            return elementType != null && CanConvertSimple( elementType );
        }

        if( toCheck.IsGenericType )
        {
            var genArgs = toCheck.GetGenericArguments();
            if( genArgs.Length != 1 )
                return false;

            if( !CanConvertSimple( genArgs[ 0 ] ) )
                return false;

            return typeof( List<> ).MakeGenericType( genArgs[ 0 ] )
                                   .IsAssignableFrom( toCheck );
        }

        if( CanConvertSimple( toCheck ) )
            return true;

        _logger?.MissingConverter( toCheck.Name );

        return false;
    }

    public ITextToValue? GetConverter( Type simpleType )
    {
        // first see if a converter exists in the converters collection
        if( _converters.TryGetValue( simpleType, out var converter ) )
            return converter;

        // next, see if we may need to add a built-in converter dynamically
        return _builtInConv switch
        {
            BuiltInConverters.AddDynamically => AddGetConverter( simpleType ),
            _ => null
        };
    }

    private ITextToValue? AddGetConverter( Type simpleType )
    {
        if( _builtInTargets == null )
        {
            _logger?.MissingBuiltInConverters();
            return null;
        }

        var builtIn = _builtInTargets.FirstOrDefault( x => x.ReturnType == simpleType );
        if( builtIn == null )
        {
            _logger?.MissingConverter( simpleType.Name );
            return null;
        }

        var builtInType = typeof( BuiltInTextToValue<> ).MakeGenericType( builtIn.ReturnType );

        var retVal = (ITextToValue?) Activator.CreateInstance( builtInType, builtIn.MethodInfo );

        if( retVal != null )
            _converters.Add( simpleType, retVal );
        else _logger?.MissingConverter( builtInType.Name );

        return retVal;
    }

    private bool CanConvertSimple( Type simpleType )
    {
        if( simpleType.IsArray || simpleType.IsGenericType )
            return false;

        if( simpleType.IsEnum )
            return true;

        return GetConverter( simpleType ) != null;
    }

    public bool Convert( Type targetType, IEnumerable<string> textValues, out object? result )
    {
        result = null;

        var converter = GetConverter( targetType );

        if( converter != null )
            return converter.Convert( textValues, out result );

        if( targetType.IsEnum )
        {
            var enumConverterType = typeof( TextToEnum<> ).MakeGenericType( targetType );
            converter = Activator.CreateInstance( enumConverterType, _logger ) as ITextToValue;
            _converters.Add( targetType, converter! );

            return converter!.Convert( textValues, out result );
        }

        _logger?.MissingConverter( targetType.Name );

        return false;
    }

    public bool TryGetValue( Type key, out ITextToValue result )
    {
        result = new UndefinedTextToValue();

        var converter = GetConverter( key );

        if( converter == null )
            return false;

        result = converter;

        return true;
    }

    public ITextToValue this[ Type key ]
    {
        get
        {
            var converter = GetConverter( key );

            if( converter != null )
                return converter;

            _logger?.MissingConverter( key.Name );
            return new UndefinedTextToValue();
        }
    }

    public IEnumerator<KeyValuePair<Type, ITextToValue>> GetEnumerator()
    {
        foreach( var kvp in _converters )
        {
            yield return kvp;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
