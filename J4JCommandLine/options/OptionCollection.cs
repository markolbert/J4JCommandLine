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
    public class OptionCollection : IOptionCollection
    {
        private readonly TypeBoundOptionComparer _comparer = new();
        private readonly IPropertyValidator _propValidator;
        private readonly IJ4JLogger? _logger;
        private readonly List<IOption> _options = new();

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
            MasterText = MasterTextCollection.GetDefault(cmdLineStyle, loggerFactory);

            LoggerFactory = loggerFactory;

            _logger = loggerFactory?.Invoke();
        }

        public OptionCollection(
            MasterTextCollection mt,
            IConverters? converters = null,
            IPropertyValidator? propValidator = null
        )
        {
            converters ??= new Converters(Enumerable.Empty<IConverter>(), mt.LoggerFactory?.Invoke());

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
            var propElements = new Stack<PropertyInfo>();

            var curExpr = propertySelector.Body;
            OptionStyle? firstStyle = null;

            while( curExpr != null )
                switch( curExpr )
                {
                    case MemberExpression memExpr:
                        var propInfo = (PropertyInfo) memExpr.Member;

                        propElements.Push(propInfo);

                        // the first PropertyInfo, which is the outermost 'leaf', must
                        // have a public parameterless constructor and a property setter
                        if ( !_propValidator.IsPropertyBindable(propElements))
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

                            propElements.Push(propInfo2);

                            // the first PropertyInfo, which is the outermost 'leaf', must
                            // have a public parameterless constructor and a property setter
                            if (!_propValidator.IsPropertyBindable(propElements))
                            //if (!ValidatePropertyInfo(propInfo2, firstStyle == null))
                                return null;

                            firstStyle ??= GetOptionStyle(propInfo2);
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

            var retVal = new TypeBoundOption<TTarget>( this, GetContextPath( propElements.ToList() ), MasterText );

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