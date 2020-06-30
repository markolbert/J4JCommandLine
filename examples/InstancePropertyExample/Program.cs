using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.CommandLine.Examples
{
    class Program
    {
        static IServiceProvider _svcProvider { get; set; }

        static void Main(string[] args)
        {
            InitializeServiceProvider();

            var builder = _svcProvider.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .HelpKeys("h", "?")
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{nameof(Program)}.exe");

            var binder = builder.Build<Configuration>( null );
            if( binder == null )
                throw new NullReferenceException(nameof(Program));

            binder!.Bind(x => x.IntValue, "i")
                .SetDescription("an integer value")
                .SetDefaultValue(1)
                .SetValidator(OptionInRange<int>.GreaterThan(0));

            binder.Bind(x => x.TextValue, "t")
                .SetDescription("a text value")
                .SetDefaultValue("some text value");

            binder.BindUnkeyed(x => x.Unkeyed);

            if (!binder.Parse(args))
            {
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine($"IntValue is {binder.Value.IntValue}");
            Console.WriteLine($"TextValue is {binder.Value.TextValue}");

            Console.WriteLine(binder.Value.Unkeyed.Count == 0
                ? "No unkeyed parameters"
                : $"Unkeyed parameters: {string.Join(", ", binder.Value.Unkeyed)}");
        }

        private static void InitializeServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FancyConsole>()
                .AsImplementedInterfaces();

            builder.AddJ4JCommandLine();

            _svcProvider = new AutofacServiceProvider(builder.Build());
        }
    }
}
