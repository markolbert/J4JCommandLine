using System;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public class TargetedProperties : KeyedCollection<string, TargetedProperty>
    {
        public TargetedProperties( bool caseSensitive = true )
            : base( caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase )
        {
        }

        protected override string GetKeyForItem( TargetedProperty item ) => item.FullPath;
    }
}