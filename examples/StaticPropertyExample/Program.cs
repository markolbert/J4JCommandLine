using System;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable 8618

namespace J4JSoftware.CommandLine.Examples;

public class Program
{
    private static J4JCommandLineBuilder? _optionBuilder;
    private static ILexicalElements? _tokens;

    static void Main( string[] args )
    {
        var builder = new HostBuilder()
                     .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                     .ConfigureHostConfiguration(SetupConfiguration)
                     .ConfigureAppConfiguration(SetupConfiguration);

        var host = builder.Build();

        if (_optionBuilder == null || _tokens == null)
        {
            Console.WriteLine("Option setup failed");

            Environment.ExitCode = -1;
            return;
        }

        var config = host.Services.GetRequiredService<IConfiguration>();
        if( config == null )
            throw new NullReferenceException( "Undefined IConfiguration" );

        var help = new ColorHelpDisplay(_tokens, _optionBuilder.Options);
        help.Display();

        var parsed = config.Get<Program>();

        if( parsed == null )
        {
            Console.WriteLine( "Parsing failed" );

            Environment.ExitCode = -1;
            return;
        }

        Console.WriteLine( "Parsing succeeded" );

        Console.WriteLine( $"IntValue is {IntValue}" );
        Console.WriteLine( $"TextValue is {TextValue}" );
        Console.WriteLine( $"SwitchValue is {SwitchValue}" );

        Console.WriteLine(_optionBuilder.Options.SpuriousValues.Count == 0
                              ? "No unkeyed parameters"
                              : $"Unkeyed parameters: {string.Join(", ", _optionBuilder.Options.SpuriousValues)}");
    }

    public static int IntValue { get; set; }
    public static string TextValue { get; set; }
    public static bool SwitchValue { get; set; }

    private static void SetupConfiguration(IConfigurationBuilder configBuilder)
    {
        _optionBuilder = new J4JCommandLineBuilder(StringComparison.OrdinalIgnoreCase);

        _optionBuilder.Bind<Program, int>( x => Program.IntValue, "i" )!
               .SetDefaultValue( 75 )
               .SetDescription( "An integer value" );

        _optionBuilder.Bind<Program, string>( x => Program.TextValue, "t" )!
            .SetDefaultValue( "a cool default" )
            .SetDescription( "A string value" );

        _optionBuilder.Bind<Program, bool>(x => Program.SwitchValue, "s")!
            .SetDefaultValue(false)
            .SetDescription("A switch");

        ILexicalElements? tokens = null;
        configBuilder.AddJ4JCommandLine(_optionBuilder, ref tokens);
        _tokens = tokens;
    }
}