using System;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public class CommandLineErrors : KeyedCollection<ErrorSource, CommandLineError>
    {
        private readonly StringComparison _keyComp;

        public CommandLineErrors( StringComparison keyComp )
        {
            _keyComp = keyComp;
        }

        public void AddError( IBindingTarget bindingTarget, string key, string error )
        {
            var source = new ErrorSource
            {
                BindingTarget = bindingTarget,
                Key = key
            };

            if( Contains( source ) )
            {
                this[ source ].Errors.Add( error );
            }
            else
            {
                var cle = new CommandLineError { Source = source };
                cle.Errors.Add( error );

                Add( cle );
            }
        }

        protected override ErrorSource GetKeyForItem( CommandLineError item )
        {
            return item.Source;
        }
    }
}