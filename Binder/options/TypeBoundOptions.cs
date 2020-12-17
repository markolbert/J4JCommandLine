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
    public class TypeBoundOptions<TTarget> : OptionsBase
        where TTarget: class, new()
    {
        public TypeBoundOptions(
            MasterTextCollection masterText,
            IJ4JLogger logger )
            : base( masterText, logger )
        {
        }

        public Type TargetType => typeof(TTarget);

        public bool Bind<TProp>( 
            Expression<Func<TTarget, TProp>> propertySelector, 
            out Option? result, bool bindNonPublic = false )
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

            result = AddInternal( GetContextPath( propElements ) );
            result.SetStyle( style!.Value );

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
                    ? OptionStyle.Collection
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