using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.CommandLine.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostConfig = new J4JHostConfiguration()
                .OperatingSystem(OSNames.Windows)
                .Publisher("J4JSoftware")
                .ApplicationName("InstancePropertyExample")
                .FilePathTrimmer(FilePathTrimmer)
                .OptionsInitializer(SetupOptions)
                .AddDependencyInjectionInitializers(SetupDependencyInjection);

            if (hostConfig.MissingRequirements != J4JHostRequirements.AllMet)
            {
                Console.WriteLine($"Missing J4JHostConfiguration items: {hostConfig.MissingRequirements}");
                Environment.ExitCode = 1;

                return;
            }

            var hostBuilder = hostConfig.CreateHostBuilder();
            if (hostBuilder == null)
            {
                Console.WriteLine($"Could not create IHostBuilder ({hostConfig.BuildStatus})");
                Environment.ExitCode = 1;

                return;
            }

            var host = hostBuilder.Build();
            if (host == null)
            {
                Console.WriteLine("Could not create IHost");
                Environment.ExitCode = 1;

                return;
            }

            var options = host.Services.GetRequiredService<IOptionCollection>();
            if (options == null)
                throw new NullReferenceException("Undefined IOptionCollection");

            var config = host.Services.GetRequiredService<IConfiguration>();
            if (config == null)
                throw new NullReferenceException("Undefined IConfiguration");

            var help = new HelpDisplayColor(options);
            help.Display();

            var parsed = config.Get<Configuration>();

            if (parsed == null)
            {
                Console.WriteLine("Parsing failed");

                Environment.ExitCode = -1;
                return;
            }

            Console.WriteLine("Parsing succeeded");

            Console.WriteLine($"IntValue is {parsed.IntValue}");
            Console.WriteLine($"TextValue is {parsed.TextValue}");

            Console.WriteLine(options.UnkeyedValues.Count == 0
                ? "No unkeyed parameters"
                : $"Unkeyed parameters: {string.Join(", ", options.UnkeyedValues)}");
        }

        private static void SetupOptions(IOptionCollection options)
        {
            options.Bind<Configuration, int>(x => x.IntValue, "i")!
                .SetDefaultValue(75)
                .SetDescription("An integer value");

            options.Bind<Configuration, string>(x => x.TextValue, "t")!
                .SetDefaultValue("a cool default")
                .SetDescription("A string value");
        }

        private static void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModule());
            builder.RegisterTextToValueAssemblies();
            builder.RegisterTokenAssemblies();
            builder.RegisterMasterTextCollectionAssemblies();
            builder.RegisterBindabilityValidatorAssemblies();
            builder.RegisterCommandLineGeneratorAssemblies();
            builder.RegisterDisplayHelpAssemblies(typeof(HelpDisplayColor));
        }

        // these next two methods serve to strip the project path off of source code
        // file paths
        private static string FilePathTrimmer(
            Type? loggedType,
            string callerName,
            int lineNum,
            string srcFilePath)
        {
            return CallingContextEnricher.DefaultFilePathTrimmer(loggedType,
                callerName,
                lineNum,
                CallingContextEnricher.RemoveProjectPath(srcFilePath, GetProjectPath()));
        }

        private static string GetProjectPath([CallerFilePath] string filePath = "")
        {
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath)!);

            while (dirInfo.Parent != null)
            {
                if (dirInfo.EnumerateFiles("*.csproj").Any())
                    break;

                dirInfo = dirInfo.Parent;
            }

            return dirInfo.FullName;
        }
    }
}
