using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

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
            return item.FullPath;
            //var retVal = item.PathElements.Aggregate( new StringBuilder(), ( sb, pi ) =>
            //{
            //    if( sb.Length > 0 )
            //        sb.Append( "." );

            //    sb.Append( pi.Name );

            //    return sb;
            //}, sb =>
            //{
            //    if( sb.Length > 0 )
            //        sb.Append( "." );

            //    sb.Append( item.PropertyInfo.Name );

            //    return sb.ToString();
            //} );

            //return retVal;
        }
    }
}