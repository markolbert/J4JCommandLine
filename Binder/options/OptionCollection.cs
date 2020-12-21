using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class OptionCollection : IEnumerable<IOption>
    {
        private class TypeBoundOptionComparer : IEqualityComparer<ITypeBoundOption>
        {
            public bool Equals(ITypeBoundOption? x, ITypeBoundOption? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                return x.TargetType.Equals(y.TargetType);
            }

            public int GetHashCode(ITypeBoundOption obj)
            {
                return obj.TargetType.GetHashCode();
            }
        }

        private readonly Dictionary<Type, string> _typePrefixes = new Dictionary<Type, string>();
        private readonly TypeBoundOptionComparer _comparer = new TypeBoundOptionComparer();
        private readonly List<IOption> _options = new();

        public OptionCollection(
            CommandLineStyle cmdLineStyle = CommandLineStyle.Windows,
            IAllocator? allocator = null,
            IElementTerminator? _elementTerminator = null,
            IKeyPrefixer? _keyPrefixer = null
        )
        {
            CommandLineStyle = cmdLineStyle;
            Log = new CommandLineLogger();
            MasterText = MasterTextCollection.GetDefault(cmdLineStyle);
            ElementTerminator = _elementTerminator ?? new ElementTerminator(MasterText, Log);
            KeyPrefixer = _keyPrefixer ?? new KeyPrefixer(MasterText, Log);
            Allocator = allocator ?? new Allocator(ElementTerminator, KeyPrefixer, Log);
        }

        public OptionCollection(
            MasterTextCollection mt,
            IAllocator? allocator = null,
            IElementTerminator? _elementTerminator = null,
            IKeyPrefixer? _keyPrefixer = null
        )
        {
            CommandLineStyle = CommandLineStyle.UserDefined;
            Log = new CommandLineLogger();
            MasterText = mt;
            ElementTerminator = _elementTerminator ?? new ElementTerminator(MasterText, Log);
            KeyPrefixer = _keyPrefixer ?? new KeyPrefixer(MasterText, Log);
            Allocator = allocator ?? new Allocator(ElementTerminator, KeyPrefixer, Log);
        }

        public CommandLineStyle CommandLineStyle { get; }
        public CommandLineLogger Log { get; }
        public IElementTerminator ElementTerminator { get; }
        public IKeyPrefixer KeyPrefixer { get; }
        public IAllocator Allocator { get; }
        public MasterTextCollection MasterText { get; }
        public ReadOnlyCollection<IOption> Options => _options.AsReadOnly();
        public int Count => _options.Count;

        public void SetTypePrefix<TTarget>(string prefix)
            where TTarget : class, new()
        {
            var type = typeof(TTarget);

            if (_typePrefixes.ContainsKey(type))
                _typePrefixes.Remove(type);

            _typePrefixes.Add(type, prefix);
        }

        public string GetTypePrefix<TTarget>()
            where TTarget : class, new()
        {
            var type = typeof(TTarget);

            if( _typePrefixes.ContainsKey( type ) )
                return $"{_typePrefixes[ type ]}:";

            return TargetsMultipleTypes ? $"{type.Name}:" : string.Empty;
        }

        public bool TargetsMultipleTypes => _options.Cast<ITypeBoundOption>().Distinct(_comparer).Count() > 1;

        public IOption Add(string contextPath)
        {
            var retVal = new Option(this, contextPath, MasterText);

            _options.Add(retVal);

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

            Expression? curExpr = propertySelector.Body;
            OptionStyle? firstStyle = null;

            while (curExpr != null)
                switch (curExpr)
                {
                    case MemberExpression memExpr:
                        var propInfo = (PropertyInfo) memExpr.Member;

                        if( !ValidateProperty( propInfo, out var curStyle ) )
                        {
                            Log.LogError( $"Property '{propInfo.Name}' is invalid");
                            return null;
                        }

                        firstStyle ??= curStyle;

                        propElements.Add(propInfo);

                        // walk up expression tree
                        curExpr = memExpr.Expression;

                        break;

                    case UnaryExpression unaryExpr:
                        if( unaryExpr.Operand is MemberExpression unaryMemExpr )
                        {
                            var propInfo2 = (PropertyInfo)unaryMemExpr.Member;

                            if (!ValidateProperty(propInfo2, out var curStyle2))
                            {
                                Log.LogError($"Property '{propInfo2.Name}' is invalid");
                                return null;
                            }

                            firstStyle ??= curStyle2;

                            propElements.Add(propInfo2);
                        }

                        // we're done; UnaryExpressions aren't part of an expression tree
                        curExpr = null;

                        break;

                    case ParameterExpression paramExpr:
                        // this is the root/anchor of the expression tree.
                        // we're done
                        curExpr = null;

                        break;
                }

            propElements.Reverse();

            var retVal = new TypeBoundOption<TTarget>(this, GetContextPath(propElements), MasterText);

            retVal.SetStyle(firstStyle!.Value);

            foreach( var key in ValidateCommandLineKeys( cmdLineKeys ) )
            {
                retVal.AddCommandLineKey( key );
            }

            _options.Add(retVal);

            return retVal;
        }

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        public bool UsesCommandLineKey( string key ) => MasterText.Contains( key, TextUsageType.OptionKey );

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

        private bool ValidateProperty(PropertyInfo propInfo, out OptionStyle? style)
        {
            style = null;

            if (!ValidateAccessMethod(propInfo.GetMethod, propInfo.Name, 0))
                return false;

            if (!ValidateAccessMethod(propInfo.SetMethod, propInfo.Name, 1))
                return false;

            if (propInfo.PropertyType.IsEnum)
            {
                style = HasAttribute<FlagsAttribute>(propInfo.PropertyType)
                    ? OptionStyle.ConcatenatedSingleValue
                    : OptionStyle.SingleValued;

                return true;
            }

            if (propInfo.PropertyType.IsGenericType)
            {
                if (ValidateGenericType(propInfo.PropertyType, out var innerStyle))
                    style = innerStyle;

                return style != null;
            }

            if (!ValidateType(propInfo.PropertyType))
                return false;

            style = propInfo.PropertyType.IsArray
                ? OptionStyle.Collection
                : typeof(bool).IsAssignableFrom(propInfo.PropertyType)
                    ? OptionStyle.Switch
                    : OptionStyle.SingleValued;

            return true;
        }

        private bool ValidateGenericType(Type genType, out OptionStyle? style)
        {
            style = null;

            if (genType.GenericTypeArguments.Length != 1)
            {
                Log.LogError($"Generic type '{genType.Name}' does not have just one generic Type argument");
                return false;
            }

            if (!ValidateType(genType.GenericTypeArguments[0]))
                return false;

            if (!typeof(List<>).MakeGenericType(genType.GenericTypeArguments[0]).IsAssignableFrom(genType))
            {
                Log.LogError($"Generic type '{genType}' is not a List<> type");
                return false;
            }

            style = OptionStyle.Collection;

            return true;
        }

        private bool ValidateType(Type toCheck)
        {
            if (toCheck.IsGenericType)
                return false;

            if (toCheck.IsArray)
                return ValidateType(toCheck.GetElementType()!);

            if( toCheck.IsValueType
                || typeof(string).IsAssignableFrom( toCheck )
                || toCheck.GetConstructors().Any( c => c.GetParameters().Length == 0 ) )
                return true;

            Log.LogError($"Unsupported type '{toCheck}'");

            return false;
        }

        private bool ValidateAccessMethod(MethodInfo? methodInfo, string propName, int allowedParams)
        {
            if (methodInfo == null)
            {
                Log.LogError($"Property '{propName}' does not have a get or set method");
                return false;
            }

            if (!methodInfo.IsPublic )
            {
                Log.LogError($"Property '{propName}::{methodInfo.Name}' is not bindable");
                return false;
            }

            if (methodInfo.GetParameters().Length > allowedParams)
            {
                Log.LogError($"Property '{propName}::{methodInfo.Name}' is indexed");
                return false;
            }

            return true;
        }

        private IEnumerable<string> ValidateCommandLineKeys( string[] cmdLineKeys )
        {
            foreach( var key in cmdLineKeys )
            {
                if( !UsesCommandLineKey( key ) )
                    yield return key;
            }
        }

        private string GetContextPath(List<PropertyInfo> propElements) =>
            propElements.Aggregate(
                new StringBuilder(),
                (sb, pi) =>
                {
                    if (sb.Length > 0)
                        sb.Append(":");

                    sb.Append(pi.Name);

                    return sb;
                },
                sb => sb.ToString()
            );

        private bool HasAttribute<TAttr>(Type toCheck)
            where TAttr : Attribute
            => toCheck.GetCustomAttribute<TAttr>() != null;
        
        public IEnumerator<IOption> GetEnumerator()
        {
            foreach( var option in _options ) yield return option;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}