using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static bool IsPublicReadWrite( this PropertyInfo propInfo, IJ4JLogger? logger )
        {
            var getAccessor = propInfo.GetGetMethod();
            var setAccessor = propInfo.GetSetMethod();

            if( getAccessor == null )
            {
                logger?.Verbose<string>( "Property {0} is not readable", propInfo.Name );
                return false;
            }

            if( setAccessor == null )
            {
                logger?.Verbose<string>( "Property {0} is not writable", propInfo.Name );
                return false;
            }

            if( !getAccessor.IsPublic )
            {
                logger?.Verbose<string>( "Property {0} is not publicly readable", propInfo.Name );
                return false;
            }

            if( getAccessor.IsAbstract || setAccessor.IsAbstract )
            {
                logger?.Verbose<string>( "Property {0} is abstract", propInfo.Name );
                return false;
            }

            if( !setAccessor.IsPublic )
            {
                logger?.Verbose<string>( "Property {0} is not publicly writable", propInfo.Name );
                return false;
            }

            if( setAccessor.IsAbstract )
            {
                logger?.Verbose<string>( "Property {0} is not writable", propInfo.Name );
                return false;
            }

            return true;
        }

        public static List<PropertyInfo> GetPropertyPathInfo<TTarget, TProp>(
            this Expression<Func<TTarget, TProp>> propertySelector)
        {
            // walk the expression tree to extract the PropertyInfo objects defining
            // the path to the property of interest
            var retVal = new List<PropertyInfo>();

            Expression? curExpr = propertySelector.Body;

            while (curExpr != null)
                switch (curExpr)
                {
                    case MemberExpression memExpr:
                        retVal.Add( (PropertyInfo) memExpr.Member );

                        // walk up expression tree
                        curExpr = memExpr.Expression;

                        break;

                    case UnaryExpression unaryExpr:
                        if (unaryExpr.Operand is MemberExpression unaryMemExpr)
                            retVal.Add((PropertyInfo)unaryMemExpr.Member);

                        // we're done; UnaryExpressions aren't part of an expression tree
                        curExpr = null;

                        break;

                    case ParameterExpression paramExpr:
                        // this is the root/anchor of the expression tree.
                        // we're done
                        curExpr = null;

                        break;
                }

            retVal.Reverse();

            return retVal;
        }

        public static bool HasPublicParameterlessConstructor( this Type toCheck )
        {
            // value types always have a public parameterless constructor
            // strings essentially do...although not technically :)
            if( toCheck.IsValueType || toCheck == typeof(string) )
                return true;

            return toCheck.GetConstructor( Type.EmptyTypes ) != null;
        }

        public static StringComparer ToStringComparer( this StringComparison textComp )
        {
            return textComp switch
            {
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.Ordinal => StringComparer.Ordinal,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                _ => throw new NotImplementedException()
            };
        }
    }
}