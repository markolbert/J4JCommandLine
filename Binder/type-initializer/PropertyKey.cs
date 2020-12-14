using System.Collections.Generic;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public class PropertyKey : IContextKey
    {
        public PropertyKey( PropertyInfo propInfo, PropertyKey? parentKey )
        {
            PropertyInfo = propInfo;
            Parent = parentKey;
        }

        public PropertyInfo PropertyInfo { get; }
        public PropertyKey? Parent { get; }
        public string Text => PropertyInfo.Name;

        public List<IContextKey>? ContextPath
        {
            get
            {
                if (Parent == null )
                    return new List<IContextKey> { this };

                var retVal = Parent.ContextPath!;

                retVal.Add( this );

                return retVal;
            }
        }

    }
}