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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class OptionCollection : IOptionCollection
    {
        private readonly TypeBoundOptionComparer _comparer = new();
        private readonly IJ4JLogger? _logger;
        private readonly List<IOption> _options = new();
        private readonly IPropertyValidator _propValidator;

        private readonly Dictionary<Type, string> _typePrefixes = new();

        public OptionCollection(
            CommandLineStyle cmdLineStyle = CommandLineStyle.Windows,
            IConverters? converters = null,
            IPropertyValidator? propValidator = null,
            Func<IJ4JLogger>? loggerFactory = null
        )
        {
            converters ??= new Converters( Enumerable.Empty<IConverter>(), loggerFactory?.Invoke() );

            _propValidator = propValidator ??
                             new DefaultPropertyValidator( converters, loggerFactory?.Invoke() );

            CommandLineStyle = cmdLineStyle;
            MasterText = MasterTextCollection.GetDefault( cmdLineStyle, loggerFactory );

            LoggerFactory = loggerFactory;

            _logger = loggerFactory?.Invoke();
        }

        public OptionCollection(
            MasterTextCollection mt,
            IConverters? converters = null,
            IPropertyValidator? propValidator = null
        )
        {
            converters ??= new Converters( Enumerable.Empty<IConverter>(), mt.LoggerFactory?.Invoke() );

            _propValidator = propValidator ??
                             new DefaultPropertyValidator( converters, mt.LoggerFactory?.Invoke() );

            CommandLineStyle = CommandLineStyle.UserDefined;
            MasterText = mt;
            LoggerFactory = mt.LoggerFactory;

            _logger = mt.LoggerFactory?.Invoke();
        }

        public Func<IJ4JLogger>? LoggerFactory { get; }
        public CommandLineStyle CommandLineStyle { get; }
        public MasterTextCollection MasterText { get; }
        public ReadOnlyCollection<IOption> Options => _options.AsReadOnly();
        public int Count => _options.Count;

        public void ClearValues()
        {
            _options.ForEach( x => x.ClearValues() );
        }

        public List<string> UnkeyedValues { get; } = new();
        public List<TokenEntry> UnknownKeys { get; } = new();

        public void SetTypePrefix<TTarget>( string prefix )
            where TTarget : class, new()
        {
            var type = typeof(TTarget);

            if( _typePrefixes.ContainsKey( type ) )
                _typePrefixes.Remove( type );

            _typePrefixes.Add( type, prefix );
        }

        public string GetTypePrefix<TTarget>()
            where TTarget : class, new()
        {
            var type = typeof(TTarget);

            if( _typePrefixes.ContainsKey( type ) )
                return $"{_typePrefixes[ type ]}:";

            return TargetsMultipleTypes ? $"{type.Name}:" : string.Empty;
        }

        public bool TargetsMultipleTypes => _options.Cast<ITypeBoundOption>().Distinct( _comparer ).Count() > 1;

        public Option<TProp>? Add<TProp>( string contextPath )
        {
            var propType = typeof(TProp);

            if( !_propValidator.IsPropertyBindable( propType ) )
            {
                _logger?.Error( "Cannot bind to type '{0}'", propType );
                return null;
            }

            if( this.Any( x => x.ContextPath!.Equals( contextPath, MasterText.TextComparison ) ) )
            {
                _logger?.Error<string>( "An option with the same ContextPath ('{0}') is already in the collection",
                    contextPath );
                return null;
            }

            var retVal = new Option<TProp>( this, contextPath, MasterText );

            _options.Add( retVal );

            return retVal;
        }

        public IOption? Add( Type propType, string contextPath )
        {
            if( !_propValidator.IsPropertyBindable( propType ) )
            {
                _logger?.Error( "Cannot bind to type '{0}'", propType );
                return null;
            }

            if( this.Any( x => x.ContextPath!.Equals( contextPath, MasterText.TextComparison ) ) )
            {
                _logger?.Error<string>( "An option with the same ContextPath ('{0}') is already in the collection",
                    contextPath );
                return null;
            }

            var genType = typeof(Option<>).MakeGenericType( propType );
            var ctor = genType.GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic )
                .FirstOrDefault();

            if( ctor == null )
            {
                _logger?.Error( "Couldn't find constructor for {0}", genType );
                return null;
            }

            var retVal = ctor.Invoke( new object?[] { this, contextPath, MasterText } ) as IOption;

            if( retVal == null )
            {
                _logger?.Error( "Failed to create instance of {0}", genType );
                return null;
            }

            _options.Add( retVal );

            return retVal;
        }

        public Option<TProp>? Bind<TContainer, TProp>(
            Expression<Func<TContainer, TProp>> propertySelector,
            params string[] cmdLineKeys )
            where TContainer : class, new()
        {
            // walk the expression tree to extract the PropertyInfo objects defining
            // the path to the property of interest
            var propElements = new Stack<PropertyInfo>();

            var curExpr = propertySelector.Body;
            OptionStyle? firstStyle = null;

            while( curExpr != null )
                switch( curExpr )
                {
                    case MemberExpression memExpr:
                        var propInfo = (PropertyInfo) memExpr.Member;

                        propElements.Push( propInfo );

                        // the first PropertyInfo, which is the outermost 'leaf', must
                        // have a public parameterless constructor and a property setter
                        if( !_propValidator.IsPropertyBindable( propElements ) )
                            //if( !ValidatePropertyInfo( propInfo, firstStyle == null ) )
                            return null;

                        firstStyle ??= GetOptionStyle( propInfo );

                        // walk up expression tree
                        curExpr = memExpr.Expression;

                        break;

                    case UnaryExpression unaryExpr:
                        if( unaryExpr.Operand is MemberExpression unaryMemExpr )
                        {
                            var propInfo2 = (PropertyInfo) unaryMemExpr.Member;

                            propElements.Push( propInfo2 );

                            // the first PropertyInfo, which is the outermost 'leaf', must
                            // have a public parameterless constructor and a property setter
                            if( !_propValidator.IsPropertyBindable( propElements ) )
                                //if (!ValidatePropertyInfo(propInfo2, firstStyle == null))
                                return null;

                            firstStyle ??= GetOptionStyle( propInfo2 );
                        }

                        // we're done; UnaryExpressions aren't part of an expression tree
                        curExpr = null;

                        break;

                    case ParameterExpression:
                        // this is the root/anchor of the expression tree.
                        // we're done
                        curExpr = null;

                        break;
                }

            var contextPath = GetContextPath( propElements.ToList() );

            if( this.Any( x => x.ContextPath!.Equals( contextPath, MasterText.TextComparison ) ) )
            {
                _logger?.Error<string>( "An option with the same ContextPath ('{0}') is already in the collection",
                    contextPath );
                return null;
            }

            var retVal = new TypeBoundOption<TContainer, TProp>(
                this,
                contextPath,
                MasterText );

            retVal.SetStyle( firstStyle!.Value );

            foreach( var key in ValidateCommandLineKeys( cmdLineKeys ) ) retVal.AddCommandLineKey( key );

            _options.Add( retVal );

            return retVal;
        }

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        public bool UsesCommandLineKey( string key )
        {
            return MasterText.Contains( key, TextUsageType.OptionKey );
        }

        public bool UsesContextPath( string contextPath )
        {
            return _options.Any( x =>
                x.ContextPath?.Equals( contextPath, MasterText.TextComparison ) ?? false );
        }

        public IOption? this[ string key ]
        {
            get
            {
                return _options.FirstOrDefault( opt =>
                    opt.IsInitialized
                    && opt.Keys.Any( k => string.Equals( k, key, MasterText.TextComparison ) )
                );
            }
        }

        public void DisplayHelp( IDisplayHelp? displayHelp = null )
        {
            displayHelp ??= new DefaultDisplayHelp( LoggerFactory?.Invoke() );

            displayHelp.ProcessOptions( this );
        }

        public bool KeysSpecified( params string[] keys )
        {
            if( keys.Length == 0 )
                return false;

            return keys.Any( k => _options.Any( x =>
                x.CommandLineKeyProvided?.Equals( k, MasterText.TextComparison ) ?? false ) );
        }

        public IEnumerator<IOption> GetEnumerator()
        {
            foreach( var option in _options ) yield return option;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<string> ValidateCommandLineKeys( string[] cmdLineKeys )
        {
            foreach( var key in cmdLineKeys )
                if( !UsesCommandLineKey( key ) )
                    yield return key;
        }

        private static string GetContextPath( List<PropertyInfo> propElements )
        {
            return propElements.Aggregate(
                new StringBuilder(),
                ( sb, pi ) =>
                {
                    if( sb.Length > 0 )
                        sb.Append( ':' );

                    sb.Append( pi.Name );

                    return sb;
                },
                sb => sb.ToString()
            );
        }

        private OptionStyle GetOptionStyle( PropertyInfo propInfo )
        {
            if( propInfo.PropertyType.IsEnum )
                return propInfo.PropertyType.GetCustomAttribute<FlagsAttribute>() != null
                    ? OptionStyle.ConcatenatedSingleValue
                    : OptionStyle.SingleValued;

            // we assume any generic type is a collection-style option because
            // the only generic types we support are List<>s
            if( propInfo.PropertyType.IsGenericType )
                return OptionStyle.Collection;

            return propInfo.PropertyType.IsArray
                ? OptionStyle.Collection
                : typeof(bool).IsAssignableFrom( propInfo.PropertyType )
                    ? OptionStyle.Switch
                    : OptionStyle.SingleValued;
        }
    }
}