using System;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.CommandLine.Examples;

class Program
{
    private static J4JCommandLineBuilder? _optionBuilder;
    private static ILexicalElements? _tokens;

    static void Main()
    {
        var builder = new HostBuilder()
                     .UseServiceProviderFactory( new AutofacServiceProviderFactory() )
                     .ConfigureHostConfiguration( SetupConfiguration )
                     .ConfigureAppConfiguration( SetupConfiguration );

        var host = builder.Build();

        if( _optionBuilder == null || _tokens == null )
        {
            Console.WriteLine( "Option setup failed" );

            Environment.ExitCode = -1;
            return;
        }

        var config = host.Services.GetRequiredService<IConfiguration>();
        if( config == null )
            throw new NullReferenceException( "Undefined IConfiguration" );

        var help = new ColorHelpDisplay( _tokens, _optionBuilder.Options );
        help.Display();

        var parsed = config.Get<Configuration>();

        if( parsed == null )
        {
            Console.WriteLine( "Parsing failed" );

            Environment.ExitCode = -1;
            return;
        }

        Console.WriteLine( "Parsing succeeded" );

        Console.WriteLine( $"IntValue is {parsed.IntValue}" );
        Console.WriteLine( $"TextValue is {parsed.TextValue}" );

        Console.WriteLine( _optionBuilder.Options.SpuriousValues.Count == 0
                               ? "No unkeyed parameters"
                               : $"Unkeyed parameters: {string.Join( ", ", _optionBuilder.Options.SpuriousValues )}" );
    }

    private static void SetupConfiguration( IConfigurationBuilder configBuilder )
    {
        _optionBuilder = new J4JCommandLineBuilder( StringComparison.OrdinalIgnoreCase );

        _optionBuilder.Bind<Configuration, int>( x => x.IntValue, "i" )!
                .SetDefaultValue( 75 )
                .SetStyle(OptionStyle.SingleValued)
                .SetDescription( "An integer value" );

        _optionBuilder.Bind<Configuration, string>( x => x.TextValue, "t" )!
                .SetDefaultValue( "a cool default" )
                .SetStyle(OptionStyle.SingleValued)
                .SetDescription( "A string value" );

        ILexicalElements? tokens = null;
        configBuilder.AddJ4JCommandLine( _optionBuilder, ref tokens );
        _tokens = tokens;
    }
}