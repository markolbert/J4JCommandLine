using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static bool IsPublicReadWrite( this PropertyInfo propInfo, IJ4JLogger? logger )
        {
            var getAccessor = propInfo.GetGetMethod();
            var setAccessor = propInfo.GetSetMethod();

            if ( getAccessor == null )
            {
                logger?.Verbose<string>("Property {0} is not readable", propInfo.Name);
                return false;
            }

            if (setAccessor == null)
            {
                logger?.Verbose<string>("Property {0} is not writable", propInfo.Name);
                return false;
            }

            if (!getAccessor.IsPublic )
            {
                logger?.Verbose<string>("Property {0} is not publicly readable", propInfo.Name);
                return false;
            }

            if (getAccessor.IsAbstract || setAccessor.IsAbstract)
            {
                logger?.Verbose<string>("Property {0} is abstract", propInfo.Name);
                return false;
            }

            if (!setAccessor.IsPublic )
            {
                logger?.Verbose<string>("Property {0} is not publicly writable", propInfo.Name);
                return false;
            }

            if (setAccessor.IsAbstract)
            {
                logger?.Verbose<string>("Property {0} is not writable", propInfo.Name);
                return false;
            }

            return true;
        }

        public static string GetPropertyPath<TTarget, TProp>(
            this Expression<Func<TTarget, TProp>> propertySelector )
        {
            // walk the expression tree to extract property path names and the property type
            var propNames = new List<string>();

            Expression? curExpr = propertySelector.Body;
            Type? propType = null;

            while (curExpr != null)
            {
                switch (curExpr)
                {
                    case MemberExpression memExpr:
                        add_target_property_name(memExpr);

                        // walk up expression tree
                        curExpr = memExpr.Expression;

                        break;

                    case UnaryExpression unaryExpr:
                        if (unaryExpr.Operand is MemberExpression unaryMemExpr)
                            add_target_property_name(unaryMemExpr);

                        // we're done; UnaryExpressions aren't part of an expression tree
                        curExpr = null;

                        break;

                    case ParameterExpression paramExpr:
                        // this is the root/anchor of the expression tree. we want 
                        // the simple type name, not the node's name
                        propNames.Add(paramExpr.Type.Name);

                        // we're done
                        curExpr = null;

                        break;
                }
            }

            propNames.Reverse();

            return string.Join( ".", propNames );

            void add_target_property_name(MemberExpression memExpr)
            {
                if (memExpr.Member is PropertyInfo propInfo)
                {
                    propNames?.Add(propInfo.Name);

                    if (propType == null)
                        propType = propInfo.PropertyType;
                }
            }
        }
    }
}
