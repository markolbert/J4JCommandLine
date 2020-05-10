using System.Collections.Generic;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public class TargetableMethod : TargetableMember
    {
        public TargetableMethod( MethodInfo methodInfo )
            : base( methodInfo )
        {
        }

        public List<ParameterInfo> Parameters { get; set; }
    }
}