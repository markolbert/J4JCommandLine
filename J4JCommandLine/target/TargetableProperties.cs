using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public class TargetableProperties : KeyedCollection<string, TargetableProperty>
    {
        public TargetableProperties( bool caseSensitive = true )
            : base( caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase )
        {
        }

        protected override string GetKeyForItem( TargetableProperty item )
        {
            return item.Path;
        }
    }
}