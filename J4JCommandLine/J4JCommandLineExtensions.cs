using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    // various extension methods used by the framework
    public static class J4JCommandLineExtensions
    {
        // determines whether a PropertyInfo object describes a publicly readable or writeable property
        public static bool IsPublicReadWrite( this PropertyInfo propInfo )
        {
            var getAccessor = propInfo.GetGetMethod();
            var setAccessor = propInfo.GetSetMethod();

            if( getAccessor == null )
                return false;

            if( setAccessor == null )
                return false;

            if( !getAccessor.IsPublic )
                return false;

            if( getAccessor.IsAbstract || setAccessor.IsAbstract )
                return false;

            if( !setAccessor.IsPublic )
                return false;

            return !setAccessor.IsAbstract;
        }

        // walks the Expression tree targeting a selected property to capture the PropertyInfo objects of
        // every parent/ancestor property
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

        // converts from StringComparison values to StringComparer values
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