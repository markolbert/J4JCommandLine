using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.Extensions.Options;

namespace J4JSoftware.CommandLine
{
    public class TypeBoundOptions : OptionsBase
    {
        private class TypeBoundOptionComparer: IEqualityComparer<ITypeBoundOption>
        {
            public bool Equals( ITypeBoundOption? x, ITypeBoundOption? y )
            {
                if( ReferenceEquals( x, y ) ) return true;
                if( ReferenceEquals( x, null ) ) return false;
                if( ReferenceEquals( y, null ) ) return false;
                if( x.GetType() != y.GetType() ) return false;
                
                return x.TargetType.Equals( y.TargetType );
            }

            public int GetHashCode( ITypeBoundOption obj )
            {
                return obj.TargetType.GetHashCode();
            }
        }
        
        private readonly Dictionary<Type, string> _typePrefixes = new Dictionary<Type, string>();
        private readonly TypeBoundOptionComparer _comparer = new TypeBoundOptionComparer();
        
        public TypeBoundOptions(
            MasterTextCollection masterText,
            IJ4JLogger logger )
            : base( masterText, logger )
        {
        }

        public void SetTypeContextKeyPrefix<TTarget>( string prefix )
            where TTarget : class, new()
        {
            var type = typeof(TTarget);

            if( _typePrefixes.ContainsKey( type ) )
                _typePrefixes.Remove( type );

            _typePrefixes.Add( type, prefix );
        }

        public string GetContextPathPrefix<TTarget>()
            where TTarget: class, new()
        {
            var type = typeof(TTarget);

            if( _typePrefixes.ContainsKey( type ) )
                return _typePrefixes[ type ];

            return type.Name;
        }

        public bool TargetsMultipleTypes => Options.Cast<ITypeBoundOption>().Distinct( _comparer ).Count() > 1;

        public bool Bind<TTarget, TProp>( 
            Expression<Func<TTarget, TProp>> propertySelector, 
            out Option? result, 
            bool bindNonPublic = false )
            where TTarget : class, new()
        {
            result = null;

            // walk the expression tree to extract the PropertyInfo objects defining
            // the path to the property of interest
            var propElements = new List<PropertyInfo>();

            Expression? curExpr = propertySelector.Body;

            while (curExpr != null)
                switch (curExpr)
                {
                    case MemberExpression memExpr:
                        propElements.Add((PropertyInfo)memExpr.Member);

                        // walk up expression tree
                        curExpr = memExpr.Expression;

                        break;

                    case UnaryExpression unaryExpr:
                        if (unaryExpr.Operand is MemberExpression unaryMemExpr)
                            propElements.Add((PropertyInfo)unaryMemExpr.Member);

                        // we're done; UnaryExpressions aren't part of an expression tree
                        curExpr = null;

                        break;

                    case ParameterExpression paramExpr:
                        // this is the root/anchor of the expression tree.
                        // we're done
                        curExpr = null;

                        break;
                }

            if( !ValidateProperty( propElements.First(), bindNonPublic, out var style ) )
                return false;

            propElements.Reverse();

            result = new TypeBoundOption<TTarget>(this, GetContextPath(propElements), MasterText);
            result.SetStyle(style!.Value);

            Options.Add(result);

            return true;
        }

        private bool ValidateProperty( PropertyInfo propInfo, bool bindNonPublic, out OptionStyle? style )
        {
            style = null;

            if( !ValidateAccessMethod( propInfo.GetMethod, bindNonPublic, propInfo.Name, 0 ) )
                return false;

            if (!ValidateAccessMethod(propInfo.SetMethod, bindNonPublic, propInfo.Name, 1))
                return false;

            if (propInfo.PropertyType.IsEnum)
            {
                style = HasAttribute<FlagsAttribute>(propInfo.PropertyType)
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
                Logger.Error( "Generic type '{0}' does not have just one generic Type argument", genType );
                return false;
            }

            if( !ValidateType( genType.GenericTypeArguments[ 0 ] ) )
                return false;

            if( !typeof(List<>).MakeGenericType( genType.GenericTypeArguments[ 0 ] ).IsAssignableFrom( genType ) )
            {
                Logger.Error("Generic type '{0}' is not a List<> type", genType);
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

            if( toCheck.IsValueType || typeof(string).IsAssignableFrom(toCheck) )
                return true;

            Logger.Error( "Unsupported type '{0}'", toCheck );

            return false;
        }

        private bool ValidateAccessMethod( MethodInfo? methodInfo, bool bindNonPublic, string propName, int allowedParams )
        {
            if (methodInfo == null)
            {
                Logger.Error<string>( "Property '{0}' does not have a get or set method", propName );
                return false;
            }

            if (!methodInfo.IsPublic && !bindNonPublic)
            {
                Logger.Error<string>("Property '{0}::{1}' is not bindable", propName, methodInfo.Name);
                return false;
            }

            if (methodInfo.GetParameters().Length > allowedParams)
            {
                Logger.Error<string>("Property '{0}::{1}' is indexed", propName, methodInfo.Name);
                return false;
            }

            return true;
        }

        private string GetContextPath( List<PropertyInfo> propElements ) =>
            propElements.Aggregate(
                new StringBuilder(),
                ( sb, pi ) =>
                {
                    if( sb.Length > 0 )
                        sb.Append( ":" );

                    sb.Append( pi.Name );

                    return sb;
                },
                sb => sb.ToString()
            );

        private bool HasAttribute<TAttr>( Type toCheck )
            where TAttr : Attribute
            => toCheck.GetCustomAttribute<TAttr>() != null;
    }
}