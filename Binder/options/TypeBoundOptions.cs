using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;

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

        public Option Bind<TProp>( Expression<Func<TTarget, TProp>> propertySelector )
        {
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

            propElements.Reverse();

            var contextPath = propElements.Aggregate(
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

            return AddInternal( contextPath );
        }
    }
}