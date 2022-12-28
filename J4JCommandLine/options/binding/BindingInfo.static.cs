// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of J4JCommandLine.
//
// J4JCommandLine is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JCommandLine is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JCommandLine. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace J4JSoftware.Configuration.CommandLine;

internal partial class BindingInfo
{
    public static BindingInfo Create<TContainer, TTarget>( Expression<Func<TContainer, TTarget>> selector )
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
                        PropertyInfo propInfo => new BindingInfo( propInfo, retVal ),
                        _                     => new BindingInfo()
                    };

                    // walk up expression tree
                    curExpr = memExpr.Expression;

                    break;

                case UnaryExpression unaryExpr:
                    if( unaryExpr.Operand is MemberExpression unaryMemExpr )
                        retVal = new BindingInfo( (PropertyInfo) unaryMemExpr.Member, retVal );

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
}