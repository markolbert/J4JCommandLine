using System;

namespace J4JSoftware.CommandLine
{
    public class TypeBoundOption<TTarget> : Option, ITypeBoundOption
        where TTarget : class, new()
    {
        internal TypeBoundOption(
            OptionCollection container,
            string typeRelativeContextPath,
            MasterTextCollection masterText
        )
            : base( container, typeRelativeContextPath, masterText )
        {
        }

        public Type TargetType => typeof(TTarget);

        public override string? ContextPath
        {
            get
            {
                if( base.ContextPath == null )
                    return null;

                return $"{Container.GetTypePrefix<TTarget>()}{base.ContextPath}";
            }
        }
    }
}