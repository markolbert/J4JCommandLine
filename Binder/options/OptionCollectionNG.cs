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
    public class OptionCollectionNG : IOptionCollection
    {
        private readonly TypeBoundOptionComparer _comparer = new();
        private readonly IJ4JLogger? _logger;
        private readonly List<IOption> _options = new();

        private readonly Dictionary<Type, string> _typePrefixes = new();

        public OptionCollectionNG(
            CommandLineStyle cmdLineStyle = CommandLineStyle.Windows,
            Func<IJ4JLogger>? loggerFactory = null
        )
        {
            CommandLineStyle = cmdLineStyle;

            var cache = new J4JLoggerCache();
            loggerFactory ??= () => new J4JCachedLogger( cache );

            _logger = loggerFactory();

            MasterText = MasterTextCollection.GetDefault( cmdLineStyle, loggerFactory );
        }

        public OptionCollectionNG( MasterTextCollection mt )
        {
            CommandLineStyle = CommandLineStyle.UserDefined;
            MasterText = mt;

            _logger = mt.LoggerFactory?.Invoke();
        }

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

        public IOption Add( string contextPath )
        {
            var retVal = new Option( this, contextPath, MasterText );

            _options.Add( retVal );

            return retVal;
        }

        public Option? Bind<TTarget, TProp>(
            Expression<Func<TTarget, TProp>> propertySelector,
            params string[] cmdLineKeys )
            where TTarget : class, new()
        {
            // walk the expression tree to extract the PropertyInfo objects defining
            // the path to the property of interest
            var propElements = new List<PropertyInfo>();

            var curExpr = propertySelector.Body;
            OptionStyle? firstStyle = null;

            while( curExpr != null )
                switch( curExpr )
                {
                    case MemberExpression memExpr:
                        var propInfo = (PropertyInfo) memExpr.Member;

                        if( !ValidateProperty( propInfo, out var curStyle ) )
                        {
                            _logger?.Error<string>( "Property '{0}' is invalid", propInfo.Name );
                            return null;
                        }

                        firstStyle ??= curStyle;

                        propElements.Add( propInfo );

                        // walk up expression tree
                        curExpr = memExpr.Expression;

                        break;

                    case UnaryExpression unaryExpr:
                        if( unaryExpr.Operand is MemberExpression unaryMemExpr )
                        {
                            var propInfo2 = (PropertyInfo) unaryMemExpr.Member;

                            if( !ValidateProperty( propInfo2, out var curStyle2 ) )
                            {
                                _logger?.Error<string>( "Property '{0}' is invalid", propInfo2.Name );
                                return null;
                            }

                            firstStyle ??= curStyle2;

                            propElements.Add( propInfo2 );
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

            propElements.Reverse();

            var retVal = new TypeBoundOption<TTarget>( this, GetContextPath( propElements ), MasterText );

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

        public IEnumerator<IOption> GetEnumerator()
        {
            foreach( var option in _options ) yield return option;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool ValidateProperty( PropertyInfo propInfo, out OptionStyle? style )
        {
            style = null;

            if( !ValidateAccessMethod( propInfo.GetMethod, propInfo.Name, 0 ) )
                return false;

            if( !ValidateAccessMethod( propInfo.SetMethod, propInfo.Name, 1 ) )
                return false;

            if( propInfo.PropertyType.IsEnum )
            {
                style = HasAttribute<FlagsAttribute>( propInfo.PropertyType )
                    ? OptionStyle.ConcatenatedSingleValue
                    : OptionStyle.SingleValued;

                return true;
            }

            if( propInfo.PropertyType.IsGenericType )
            {
                if( ValidateGenericType( propInfo.PropertyType, out var innerStyle ) )
                    style = innerStyle;

                return style != null;
            }

            if( !ValidateType( propInfo.PropertyType ) )
                return false;

            style = propInfo.PropertyType.IsArray
                ? OptionStyle.Collection
                : typeof(bool).IsAssignableFrom( propInfo.PropertyType )
                    ? OptionStyle.Switch
                    : OptionStyle.SingleValued;

            return true;
        }

        private bool ValidateGenericType( Type genType, out OptionStyle? style )
        {
            style = null;

            if( genType.GenericTypeArguments.Length != 1 )
            {
                _logger?.Error<string>( "Generic type '{0}' does not have just one generic Type argument",
                    genType.Name );

                return false;
            }

            if( !ValidateType( genType.GenericTypeArguments[ 0 ] ) )
                return false;

            if( !typeof(List<>).MakeGenericType( genType.GenericTypeArguments[ 0 ] ).IsAssignableFrom( genType ) )
            {
                _logger?.Error( "Generic type '{0}' is not a List<> type", genType );
                return false;
            }

            style = OptionStyle.Collection;

            return true;
        }

        private bool ValidateType( Type toCheck )
        {
            if( toCheck.IsGenericType )
                return false;

            if( toCheck.IsArray )
                return ValidateType( toCheck.GetElementType()! );

            if( toCheck.IsValueType
                || typeof(string).IsAssignableFrom( toCheck )
                || toCheck.GetConstructors().Any( c => c.GetParameters().Length == 0 ) )
                return true;

            _logger?.Error( "Unsupported type '{0}'", toCheck );

            return false;
        }

        private bool ValidateAccessMethod( MethodInfo? methodInfo, string propName, int allowedParams )
        {
            if( methodInfo == null )
            {
                _logger?.Error<string>( "Property '{0}' does not have a get or set method", propName );
                return false;
            }

            if( !methodInfo.IsPublic )
            {
                _logger?.Error<string, string>( "Property '{0}::{1}' is not bindable", propName, methodInfo.Name );
                return false;
            }

            if( methodInfo.GetParameters().Length > allowedParams )
            {
                _logger?.Error<string>( "Property '{0}::{1}' is indexed", propName, methodInfo.Name );
                return false;
            }

            return true;
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
                        sb.Append(':');

                    sb.Append( pi.Name );

                    return sb;
                },
                sb => sb.ToString()
            );
        }

        private static bool HasAttribute<TAttr>( Type toCheck )
            where TAttr : Attribute
        {
            return toCheck.GetCustomAttribute<TAttr>() != null;
        }

        private class TypeBoundOptionComparer : IEqualityComparer<ITypeBoundOption>
        {
            public bool Equals( ITypeBoundOption? x, ITypeBoundOption? y )
            {
                if( ReferenceEquals( x, y ) ) return true;
                if( x is null) return false;
                if( y is null) return false;

                return x.GetType() == y.GetType() && x.TargetType == y.TargetType;
            }

            public int GetHashCode( ITypeBoundOption obj )
            {
                return obj.TargetType.GetHashCode();
            }
        }
    }
}