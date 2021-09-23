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

#pragma warning disable 8618

namespace J4JSoftware.CommandLine.Examples
{
    public class Program
    {
        static void Main(string[] args)
        {
            var hostConfig = new J4JHostConfiguration()
                .Publisher("J4JSoftware")
                .ApplicationName("StaticPropertyExample")
                .FilePathTrimmer(FilePathTrimmer);

            hostConfig.AddCommandLineProcessing( CommandLineOperatingSystems.Windows )
                .OptionsInitializer( SetupOptions );

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

            var hostInfo = host.Services.GetRequiredService<J4JHostInfo>();

            var help = new HelpDisplayColor(hostInfo.CommandLineLexicalElements!, options);
            help.Display();

            var parsed = config.Get<Program>();

            if (parsed == null)
            {
                Console.WriteLine("Parsing failed");

                Environment.ExitCode = -1;
                return;
            }

            Console.WriteLine("Parsing succeeded");

            Console.WriteLine($"IntValue is {IntValue}");
            Console.WriteLine($"TextValue is {TextValue}");

            Console.WriteLine(options.SpuriousValues.Count == 0
                ? "No unkeyed parameters"
                : $"Unkeyed parameters: {string.Join(", ", options.SpuriousValues)}");
        }

        public static int IntValue { get; set; }
        public static string TextValue { get; set; }

        private static void SetupOptions(IOptionCollection options)
        {
            options.Bind<Program, int>(x => Program.IntValue, "i")!
                .SetDefaultValue(75)
                .SetDescription("An integer value");

            options.Bind<Program, string>(x => Program.TextValue, "t")!
                .SetDefaultValue("a cool default")
                .SetDescription("A string value");
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
