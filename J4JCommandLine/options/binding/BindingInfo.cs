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
using System.Reflection;
using System.Text;

namespace J4JSoftware.Configuration.CommandLine;

internal partial class BindingInfo
{
    private BindingInfo()
    {
    }

    private BindingInfo( PropertyInfo propInfo,
        BindingInfo? child )
    {
        TypeNature = propInfo.PropertyType.GetTypeNature();
        OptionStyle = GetOptionStyle( propInfo.PropertyType );
        ConversionType = propInfo.PropertyType.GetTargetType();
        Name = propInfo.Name;
        IsProperty = true;
        MeetsSetRequirements = HasUnindexedPublicSetMethod( propInfo );
        MeetsGetRequirements = HasPublicGetMethod( propInfo );
        Child = child;

        if( child != null )
            child.Parent = this;
    }

    private bool HasUnindexedPublicSetMethod( PropertyInfo propInfo )
    {
        var setMethod = propInfo.GetSetMethod();

        return setMethod != null
         && setMethod.IsPublic
         && setMethod.GetParameters().Length == 1;
    }

    private bool HasPublicGetMethod( PropertyInfo propInfo )
    {
        var getMethod = propInfo.GetGetMethod();

        return getMethod != null && getMethod.IsPublic;
    }

    private CommandLine.OptionStyle GetOptionStyle( Type propertyType )
    {
        if( propertyType.IsEnum )
            return propertyType.IsFlaggedEnum() ? OptionStyle.ConcatenatedSingleValue : OptionStyle.SingleValued;

        if( propertyType.IsBindableCollection() )
        {
            var elementType = propertyType.GetBindableCollectionElement()!;

            if( elementType.IsFlaggedEnum() || elementType.IsBindableCollection() )
                return OptionStyle.Undefined;

            return OptionStyle.Collection;
        }

        return typeof( bool ).IsAssignableFrom( propertyType ) ? OptionStyle.Switch : OptionStyle.SingleValued;
    }

    public Type? ConversionType { get; private set; }
    public ITextToValue? Converter { get; internal set; }
    public string Name { get; private set; } = string.Empty;

    public string FullName
    {
        get
        {
            var curBI = Root;
            var sb = new StringBuilder();

            while( curBI != null )
            {
                if( sb.Length > 0 )
                    sb.Append( ":" );

                sb.Append( curBI.Name );

                curBI = curBI.Child;
            }

            return sb.ToString();
        }
    }

    public TypeNature TypeNature { get; private set; }
    public bool IsProperty { get; private set; }
    public OptionStyle OptionStyle { get; private set; } = OptionStyle.Undefined;

    public bool MeetsSetRequirements { get; private set; }
    public bool MeetsGetRequirements { get; private set; }

    public BindingInfo? Parent { get; private set; }
    public BindingInfo? Child { get; private set; }

    public bool IsRoot => Parent == null;
    public bool IsOutermostLeaf => ( Parent != null && Child == null );

    public BindingInfo Root => Parent == null ? this : Parent.Root;
    public BindingInfo OutermostLeaf => Child == null ? this : Child.OutermostLeaf;
}