using System;

namespace J4JSoftware.Configuration.CommandLine
{
    public class TypeBoundOption<TContainer, TProp> : Option<TProp>, ITypeBoundOption
        where TContainer : class, new()
    {
        internal TypeBoundOption(
            IOptionCollection container,
            string typeRelativeContextPath,
            MasterTextCollection masterText
        )
            : base( container, typeRelativeContextPath, masterText )
        {
        }

        public Type ContainerType => typeof(TContainer);

        public override string? ContextPath
        {
            get
            {
                if( base.ContextPath == null )
                    return null;

                return $"{Container.GetTypePrefix<TContainer>()}{base.ContextPath}";
            }
        }
    }
}