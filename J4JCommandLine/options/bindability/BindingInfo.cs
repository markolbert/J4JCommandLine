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
using System.Reflection;

namespace J4JSoftware.Configuration.CommandLine
{
    public enum BindingTarget
    {
        Field,
        Property
    }

    internal abstract record BindingInfo
    {
        protected BindingInfo(
            BindableType bindableType,
            BindingTarget target
        )
        {
            BindableType = bindableType;
            Target = target;
        }

        public abstract Type TargetType { get; }
        public abstract string Name { get; }
        public BindableType BindableType { get; }
        public BindingTarget Target { get; }
    }

    internal record BoundPropertyInfo( BindableType BindableType, PropertyInfo PropertyInfo )
        : BindingInfo( BindableType, BindingTarget.Property )
    {
        public override Type TargetType => PropertyInfo.PropertyType;
        public override string Name => PropertyInfo.Name;
    }

    internal record BoundFieldInfo( BindableType BindableType, FieldInfo FieldInfo )
        : BindingInfo( BindableType, BindingTarget.Field )
    {
        public override Type TargetType => FieldInfo.FieldType;
        public override string Name => FieldInfo.Name;
    }
}