using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class J4JCommandLineBuilder
{
    private readonly List<Func<BindingInfo, bool>> _bindingTests;
    private readonly ILogger? _logger = CommandLineLoggerFactory.Default.Create<J4JCommandLineBuilder>();

    public J4JCommandLineBuilder(
        StringComparison textComparison,
        CommandLineOperatingSystems os = CommandLineOperatingSystems.Windows,
        ITextConverters? converters = null
    )
    {
        TextComparison = textComparison;
        Os = os;

        Options = new OptionCollection( textComparison, converters ?? new TextConverters() );

        _bindingTests =
        [
            BindingSupported,
            CanConvert,
            HasParameterlessConstructor,
            HasAccessibleGetter,
            HasAccessibleSetter
        ];
    }

    public OptionCollection Options { get; }

    public StringComparison TextComparison { get; }
    public CommandLineOperatingSystems Os { get; }
    public bool IncludeCommonLexicalElements { get; set; } = true;
    public List<ICleanupTokens> CleanupTokens { get; } = [];

    public bool TryBind<TContainer, TTarget>(
        Expression<Func<TContainer, TTarget>> selector,
        out Option<TContainer, TTarget>? option,
        params string[] cmdLineKeys
    )
        where TContainer : class, new()
    {
        option = Bind( selector, cmdLineKeys );

        return option != null;
    }

    public Option<TContainer, TTarget>? Bind<TContainer, TTarget>(
        Expression<Func<TContainer, TTarget>> expr,
        params string[] cmdLineKeys
    )
        where TContainer : class, new()
    {
        var bindingInfo = BindingInfo.Create( expr );
        if( !bindingInfo.IsProperty )
        {
            _logger?.InvalidBindingTarget( bindingInfo.FullName );
            return null;
        }

        bindingInfo.Converter = Options.Converters[ typeof( TTarget ) ];

        var curBindingInfo = bindingInfo.Root;

        while( curBindingInfo != null )
        {
            foreach( var test in _bindingTests )
            {
                if( test( bindingInfo ) )
                    continue;

                _logger?.BindingFailed( curBindingInfo.FullName, test.GetMethodInfo().Name );
                return null;
            }

            curBindingInfo = curBindingInfo.Child;
        }

        if( Options.OptionsInternal.Any( x => x.ContextPath!.Equals( bindingInfo.FullName, TextComparison ) ) )
        {
            _logger?.DuplicateBinding( bindingInfo.FullName );
            return null;
        }

        var retVal = new Option<TContainer, TTarget>( Options, bindingInfo.FullName, bindingInfo.Converter! );

        retVal.SetStyle( bindingInfo.OutermostLeaf.OptionStyle );

        foreach( var key in ValidateCommandLineKeys( cmdLineKeys ) )
        {
            retVal.AddCommandLineKey( key );
        }

        Options.OptionsInternal.Add( retVal );

        return retVal;
    }

    private IEnumerable<string> ValidateCommandLineKeys( string[] cmdLineKeys )
    {
        foreach( var key in cmdLineKeys )
        {
            if( !Options.CommandLineKeyInUse( key ) )
                yield return key;
        }
    }

    private bool BindingSupported( BindingInfo toTest ) => toTest.TypeNature != TypeNature.Unsupported;
    private bool HasAccessibleGetter( BindingInfo toTest ) => toTest.MeetsGetRequirements;

    private bool HasAccessibleSetter( BindingInfo toTest ) => !toTest.IsOutermostLeaf || toTest.MeetsSetRequirements;

    private bool CanConvert( BindingInfo toTest )
    {
        if( !toTest.IsOutermostLeaf )
            return true;

        if( toTest.ConversionType == null )
            return false;

        toTest.Converter = Options.Converters
                                  .FirstOrDefault( x => x.Value.CanConvert( toTest.ConversionType ) )
                                  .Value;

        return toTest.Converter != null;
    }

    private bool HasParameterlessConstructor( BindingInfo toTest ) =>
        toTest.Parent == null
     || ( toTest.Parent.ConversionType != null
         && toTest.Parent.ConversionType.GetConstructors().Any( x => x.GetParameters().Length == 0 ) );
}
