#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Test.J4JCommandLine' is free software: you can redistribute it
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
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;

namespace J4JSoftware.Binder.Tests
{
    public static class BindingExtensions
    {
        public static IOption Bind<TTarget, TProp>(this TTarget target, IParser parser, Expression<Func<TTarget, TProp>> propSelector, TestConfig testConfig )
            where TTarget : class, new()
        {
            var option = parser.Options.Bind(propSelector);
            option.Should().NotBeNull();

            var optConfig = testConfig.OptionConfigurations
                .FirstOrDefault(x =>
                    option!.ContextPath!.Equals(x.ContextPath, StringComparison.OrdinalIgnoreCase));

            optConfig.Should().NotBeNull();

            option!.AddCommandLineKey(optConfig!.CommandLineKey)
                .SetStyle(optConfig.Style);

            if (optConfig.Required) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;

            return option;
        }

        public static IOption Bind<TTarget, TProp>( this TTarget target, IParser parser, Expression<Func<TTarget, TProp>> propSelector )
            where TTarget : class, new()
        {
            //propSelector.Body.Should().BeAssignableTo<MemberExpression>();
            //var expression = (MemberExpression)propSelector.Body;

            //expression.Expression.Should().NotBeNull();

            var option = parser.Options.Bind(propSelector);
            option.Should().NotBeNull();

            return option!;
        }

    }
}