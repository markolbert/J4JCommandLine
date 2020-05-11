using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class BindingTargetFactory : IBindingTargetFactory
    {
        private readonly ITargetingConfiguration _config;
        private readonly IOptionFactory _optionFactory;
        private readonly IJ4JLogger? _logger;

        public BindingTargetFactory(
            ITargetingConfiguration config,
            IOptionFactory optionFactory,
            IJ4JLogger? logger = null
        )
        {
            _config = config;
            _optionFactory = optionFactory;
            _logger = logger;
        }

        public BindingTarget<T> Create<T>( T? target = null )
            where T : class
        {
            if( target == null )
                return new BindingTarget<T>( _config, _optionFactory, _logger );

            return new BindingTarget<T>( target, _config, _optionFactory, _logger );
        }
    }
}