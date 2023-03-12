// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of J4JCommandLine.
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Serilog;

namespace J4JSoftware.Configuration.CommandLine;

public class OptionCollection
{
    public static OptionCollection GetWindowsDefault( ILogger? logger = null ) =>
        new OptionCollection( StringComparison.OrdinalIgnoreCase, new TextConverters( logger: logger ), logger );

    public static OptionCollection GetLinuxDefault( ILogger? logger = null ) =>
        new OptionCollection( StringComparison.Ordinal, new TextConverters( logger: logger ), logger );

    public event EventHandler? Configured;

    private readonly StringComparison _textComparison;
    private readonly ITextConverters _converters;
    private readonly List<Func<BindingInfo, bool>> _bindingTests;
    private readonly ILogger? _logger;

    public OptionCollection( StringComparison textComparison,
        ITextConverters converters,
        ILogger? logger = null )
    {
        _textComparison = textComparison;
        _converters = converters;

        _logger = logger;
        _logger?.ForContext<OptionCollection>();

        _bindingTests = new List<Func<BindingInfo, bool>>
        {
            BindingSupported,
            CanConvert,
            HasParameterlessConstructor,
            HasAccessibleGetter,
            HasAccessibleSetter
        };
    }

    internal List<IOptionInternal> OptionsInternal { get; } = new();

    public bool IsConfigured { get; private set; }

    public void FinishConfiguration()
    {
        IsConfigured = true;
        Configured?.Invoke( this, EventArgs.Empty );
    }

    public ReadOnlyCollection<IOption> Options => OptionsInternal.Cast<IOption>().ToList().AsReadOnly();
    public int Count => OptionsInternal.Count;

    public void ClearValues()
    {
        OptionsInternal.ForEach( x => x.ClearValues() );
    }

    // values associated with an option but in excess of the maximum number
    // of allowed values (which can be zero for things like switches which
    // do not accept or require values)
    public List<string> SpuriousValues { get; } = new();

    // things that parsed as valid keys but which do not match any key
    // configured in the collection (probably a sign of a misconfiguration
    // of IOptionCollection)
    public List<CommandLineArgument> UnknownKeys { get; } = new();

    public bool TryBind<TContainer, TTarget>( Expression<Func<TContainer, TTarget>> selector,
        out Option<TContainer, TTarget>? option,
        params string[] cmdLineKeys )
        where TContainer : class, new()
    {
        option = Bind( selector, cmdLineKeys );

        return option != null;
    }

    public Option<TContainer, TTarget>? Bind<TContainer, TTarget>( Expression<Func<TContainer, TTarget>> selector,
        params string[] cmdLineKeys )
        where TContainer : class, new()
    {
        var bindingInfo = BindingInfo.Create( selector );
        if( !bindingInfo.IsProperty )
        {
            _logger?.Error( "Binding target {0} is not a property", bindingInfo.FullName );
            return null;
        }

        bindingInfo.Converter = _converters[typeof(TTarget)];

        var curBindingInfo = bindingInfo.Root;

        while( curBindingInfo != null )
        {
            foreach ( var test in _bindingTests )
            {
                if( test( bindingInfo ) )
                    continue;

                _logger?.Error( "{0} failed {1}", curBindingInfo.FullName, test );
                return null;
            }

            curBindingInfo = curBindingInfo.Child;
        }

        if( OptionsInternal.Any( x => x.ContextPath!.Equals( bindingInfo.FullName, _textComparison ) ) )
        {
            _logger?.Error( "An option with the same ContextPath ('{0}') is already in the collection",
                                    bindingInfo.FullName );
            return null;
        }

        var retVal = new Option<TContainer, TTarget>( this, bindingInfo.FullName, bindingInfo.Converter! );

        retVal.SetStyle( bindingInfo.OutermostLeaf.OptionStyle );

        foreach( var key in ValidateCommandLineKeys( cmdLineKeys ) )
        {
            retVal.AddCommandLineKey( key );
        }

        OptionsInternal.Add( retVal );

        return retVal;
    }

    private bool BindingSupported( BindingInfo toTest ) => toTest.TypeNature != TypeNature.Unsupported;
    private bool HasAccessibleGetter( BindingInfo toTest ) => toTest.MeetsGetRequirements;

    private bool HasAccessibleSetter( BindingInfo toTest ) =>
        !toTest.IsOutermostLeaf || toTest.MeetsSetRequirements;

    private bool CanConvert( BindingInfo toTest )
    {
        if ( !toTest.IsOutermostLeaf )
            return true;

        if ( toTest.ConversionType == null )
            return false;

        toTest.Converter = _converters
                          .FirstOrDefault( x => x.Value.CanConvert( toTest.ConversionType ) )
                          .Value;

        return toTest.Converter != null;
    }

    private bool HasParameterlessConstructor( BindingInfo toTest ) =>
        toTest.Parent == null
     || ( toTest.Parent.ConversionType != null
         && toTest.Parent.ConversionType.GetConstructors().Any( x => x.GetParameters().Length == 0 ) );

    // determines whether or not a key is being used by an existing option, honoring whatever
    // case sensitivity is in use
    public bool CommandLineKeyInUse( string key )
    {
        foreach( var option in OptionsInternal )
        {
            if( option.Keys.Any( x => x.Equals( key, _textComparison ) ) )
                return true;
        }

        return false;
    }

    public IOption? this[ string key ]
    {
        get
        {
            return OptionsInternal.FirstOrDefault( opt =>
                                                       opt.IsInitialized
                                                    && opt.Keys.Any( k => string.Equals( k,
                                                                         key,
                                                                         _textComparison ) ) );
        }
    }

    public bool KeysSpecified( params string[] keys )
    {
        if( keys.Length == 0 )
            return false;

        return keys.Any( k => OptionsInternal.Any( x =>
                                                       x.CommandLineKeyProvided?.Equals( k, _textComparison )
                                                    ?? false ) );
    }

    private IEnumerable<string> ValidateCommandLineKeys( string[] cmdLineKeys )
    {
        foreach( var key in cmdLineKeys )
        {
            if( !CommandLineKeyInUse( key ) )
                yield return key;
        }
    }
}