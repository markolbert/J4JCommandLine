using System.Reflection;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class TargetableProperty
    {
        public TargetableProperty( PropertyInfo propertyInfo )
        {
            PropertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo { get; }
        public IOption? BoundOption { get; set; }

        public string Path
        {
            get
            {
                var retVal = new StringBuilder(PropertyInfo.Name);

                var curParent = PropertyInfo.DeclaringType;

                while( curParent != null )
                {
                    retVal.Insert( 0, $"{curParent.Name}." );

                    curParent = curParent.DeclaringType;
                }

                return retVal.ToString();
            }
        }
    }
}