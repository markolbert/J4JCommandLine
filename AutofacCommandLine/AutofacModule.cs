using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using J4JSoftware.Logging;
using Module = Autofac.Module;

namespace J4JSoftware.Configuration.CommandLine
{
    public class AutofacModule : Module
    {
        protected override void Load( ContainerBuilder builder )
        {
            base.Load( builder );

            builder.RegisterType<ParserFactory>()
                .As<IParserFactory>()
                .SingleInstance();

            RegisterBuiltInTextToValue( builder );
        }

        private void RegisterBuiltInTextToValue( ContainerBuilder builder )
        {
            foreach( var convMethod in typeof(Convert)
                .GetMethods( BindingFlags.Static | BindingFlags.Public )
                .Where( m =>
                {
                    var parameters = m.GetParameters();

                    return parameters.Length == 1 && !typeof(string).IsAssignableFrom( parameters[ 0 ].ParameterType );
                } ) )
            {
                builder.Register( c =>
                {
                    var logger = c.IsRegistered<IJ4JLogger>() ? c.Resolve<IJ4JLogger>() : null;

                    var builtInType = typeof(BuiltInTextToValue<>).MakeGenericType( convMethod.ReturnType );

                    return (ITextToValue) Activator.CreateInstance( builtInType, new object?[] { convMethod, logger } )!;
                } );
            }
        }
    }
}
