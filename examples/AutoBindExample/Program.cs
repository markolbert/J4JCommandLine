using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBindExample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeServiceProvider();

            var builder = ServiceProvider.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .HelpKeys("h", "?")
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{nameof(Program)}.exe");

            var binder = builder.AutoBind<Program>();
            if (binder == null)
                throw new NullReferenceException(nameof(Program));

            binder.Options[ "i" ]!.SetValidator( OptionInRange<int>.GreaterThan( 0 ) );

            if (!binder.Parse(args))
            {
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine($"IntValue is {IntValue}");
            Console.WriteLine($"TextValue is {TextValue}");

            Console.WriteLine(Unkeyed.Count == 0
                ? "No unkeyed parameters"
                : $"Unkeyed parameters: {string.Join(", ", Unkeyed)}");
        }

        public static IServiceProvider ServiceProvider { get; set; }

        [OptionKeys("i")]
        [DefaultValue(5)]
        [Description("some integer value")]
        public static int IntValue { get; set; }

        [OptionKeys("t")]
        [DefaultValue("abc")]
        [Description("some text value")]
        public static string TextValue { get; set; }

        [OptionKeys()]
        public static List<string> Unkeyed { get; set; }

        private static void InitializeServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FancyConsole>()
                .AsImplementedInterfaces();

            builder.AddJ4JCommandLine();

            ServiceProvider = new AutofacServiceProvider(builder.Build());
        }
    }
}
