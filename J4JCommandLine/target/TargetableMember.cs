using System.Reflection;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class TargetableMember
    {
        public TargetableMember( MemberInfo memberInfo )
        {
            MemberInfo = memberInfo;
        }

        public MemberInfo MemberInfo { get; }

        public string Path
        {
            get
            {
                var retVal = new StringBuilder(MemberInfo.Name);

                var curParent = MemberInfo.DeclaringType;

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