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
using System.Linq.Expressions;
using System.Reflection;

namespace J4JSoftware.Configuration.CommandLine
{
    internal interface IBindableTest
    {
        bool IsBindable( BindingInfo bindingInfo );
    }

    internal class AllSupportedTest : IBindableTest
    {
        public bool IsBindable( BindingInfo bindingInfo )
        {
            var curBI = bindingInfo.Root;

            while( curBI != null )
            {
                if( curBI.BindableType == BindableType.Unsupported )
                    return false;

                curBI = curBI.Child;
            }

            return true;
        }
    }

    internal class AccessibleGettersTest : IBindableTest
    {
        public bool IsBindable( BindingInfo bindingInfo )
        {
            var curBI = bindingInfo.Root;

            if( curBI.Target == BindingTarget.Field )
                return true;

            while( curBI != null )
            {
                curBI = curBI.Child;
            }

            return true;
        }
    }

    internal class BindingInfo
    {
        public static BindingInfo Create<TContainer, TTarget>(
            Expression<Func<TContainer, TTarget>> selector
        )
        {
            var curExpr = selector.Body;
            BindingInfo? retVal = null;

            while( curExpr != null )
            {
                switch( curExpr )
                {
                    case MemberExpression memExpr:
                        retVal = memExpr.Member switch
                        {
                            PropertyInfo propInfo => CreateProperty( propInfo, retVal ),
                            FieldInfo fieldInfo => CreateField( fieldInfo ),
                            _ => CreateUnsupported()
                        };

                        // walk up expression tree
                        curExpr = memExpr.Expression;

                        break;

                    case UnaryExpression unaryExpr:
                        if( unaryExpr.Operand is MemberExpression unaryMemExpr )
                            retVal = CreateProperty( (PropertyInfo)unaryMemExpr.Member, retVal );

                        // we're done; UnaryExpressions aren't part of an expression tree
                        curExpr = null;

                        break;

                    case ParameterExpression:
                        // this is the root/anchor of the expression tree.
                        // we're done
                        curExpr = null;

                        break;
                }
            }

            return retVal!;
        }

        private static BindingInfo CreateProperty( PropertyInfo propInfo, BindingInfo? child )
        {
            var retVal = new BindingInfo
            {
                BindableType = propInfo.PropertyType.GetBindingType(),
                Name = propInfo.Name,
                Target = BindingTarget.Property,
                TargetType = propInfo.PropertyType,
                Child = child
            };

            if( child != null )
                child.Parent = retVal;

            return retVal;
        }

        private static BindingInfo CreateField( FieldInfo fieldInfo ) =>
            new BindingInfo
            {
                BindableType = fieldInfo.FieldType.GetBindingType(),
                Name = fieldInfo.Name,
                Target = BindingTarget.Field,
                TargetType = fieldInfo.FieldType
            };

        private static BindingInfo CreateUnsupported() =>
            new BindingInfo
            {
                BindableType = BindableType.Unsupported,
                Name = string.Empty,
                Target = BindingTarget.Unsupported,
                TargetType = typeof( object )
            };

        private BindingInfo()
        {
        }

        public Type TargetType { get; private set; }
        public string Name { get; private set; }
        public BindableType BindableType { get; private set; }
        public BindingTarget Target { get; private set; }

        public BindingInfo? Parent { get; private set; }
        public BindingInfo? Child { get; private set; }

        public bool IsOutermostLeaf => Target == BindingTarget.Field
                                       || ( Parent != null && Child == null );

        public BindingInfo Root => Parent == null ? this : Parent.Root;
        public BindingInfo OutermostLeaf => Child == null ? this : Child.OutermostLeaf;
    }
}