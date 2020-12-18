using System;

namespace J4JSoftware.CommandLine
{
    public class TypeBoundOption<TTarget> : Option, ITypeBoundOption
        where TTarget : class, new()
    {
        internal TypeBoundOption(
            TypeBoundOptions container,
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

                var castContainer = (TypeBoundOptions) Container;

                return castContainer.TargetsMultipleTypes
                    ? $"{castContainer.GetContextPathPrefix<TTarget>()}:{base.ContextPath}"
                    : base.ContextPath;
            }
        }
    }
}