using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.CommandLine
{
    public static class ConfigurationExtensions
    {
        public static J4JConfigurationSource AddMapping(
            this J4JConfigurationSource src,
            string optionKey,
            string configKey )
        {
            var option = src.BindingTarget
                .Options
                .FirstOrDefault( x => x.Keys.Any(
                    k => k.Equals( optionKey, StringComparison.OrdinalIgnoreCase ) ) );

            if( option == null )
                return src;

            if( src.ConfigurationMap.ContainsKey( optionKey ) )
                src.ConfigurationMap[ optionKey ] = configKey;
            else src.ConfigurationMap.Add( optionKey, configKey );

            return src;
        }

        public static J4JConfigurationSource UsingArguments( this J4JConfigurationSource src, string[] args )
        {
            src.Arguments = args;

            return src;
        }

        public class J4JConfigurationSource : IConfigurationSource
        {
            private string[]? _args;

            public J4JConfigurationSource( IBindingTarget bindingTgt )
            {
                BindingTarget = bindingTgt;
            }

            public IBindingTarget BindingTarget { get; }
            internal Dictionary<string, string> ConfigurationMap { get; } = new Dictionary<string, string>();

            public string[] Arguments
            {
                get => _args ?? Environment.GetCommandLineArgs();
                internal set => _args = value;
            }

            public IConfigurationProvider Build( IConfigurationBuilder builder )
            {
                return new J4JConfigurationProvider( this );
            }
        }

        public class J4JConfigurationProvider : ConfigurationProvider
        {
            public J4JConfigurationProvider( J4JConfigurationSource source )
            {
                Source = source;
            }

            public J4JConfigurationSource Source { get; }

            public override void Load()
            {
                var allocations = Source.BindingTarget.AllocateCommandLineArguments( Source.Arguments );

                foreach( var option in Source.BindingTarget.Options )
                {
                    foreach( var key in option.Keys )
                    {

                    }
                }
            }
        }
    }
}
