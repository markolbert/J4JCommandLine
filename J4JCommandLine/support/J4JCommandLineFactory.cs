using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine.support
{
    public class J4JCommandLineFactory
    {
        private record TypeInfo(
            string OperatingSystem,
            Customization Customization,
            int Priority,
            Type Type
        );

        private static Type[] _supportedTypes = new[]
        {
            typeof( ITextToValue ),
            typeof(IMasterTextCollection),
            typeof(IAvailableTokens),
            typeof(IOptionsGenerator),
        };

        private readonly List<Type> _exportedTypes;
        private readonly Dictionary<string, List<string>> _osSynonyms = new(StringComparer.OrdinalIgnoreCase);
        private readonly Func<IJ4JLogger>? _loggerFactory;
        private readonly IJ4JLogger? _logger;

        private readonly List<TypeInfo> _textToValue;
        private readonly List<TypeInfo> _tokens;
        private readonly List<TypeInfo> _masterText;
        private readonly List<TypeInfo> _bindabilityValidators;
        private readonly List<TypeInfo> _optionGenerators;

        public J4JCommandLineFactory(
            IEnumerable<Assembly> assemblies,
            Func<IJ4JLogger>? loggerFactory
        )
        {
            var assemblyList = assemblies.ToList();
            assemblyList.Add( GetType().Assembly );
            assemblyList = assemblyList.Distinct().ToList();

            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( GetType() );

            _exportedTypes = assemblyList.SelectMany( x => x.ExportedTypes )
                .Where( x => _supportedTypes.Any( y => y.IsAssignableFrom( x ) )
                             && x.GetConstructors().Any() )
                .ToList();

            _textToValue = GetTypeInfos<ITextToValue>(
                PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired );

            _tokens = GetTypeInfos<IAvailableTokens>(
                PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired );

            _masterText = GetTypeInfos<IMasterTextCollection>(
                PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired );

            _bindabilityValidators = GetTypeInfos<IBindabilityValidator>(
                TypeTester.NonAbstract,
                new ConstructorTesterPermuted<IBindabilityValidator>( 
                    typeof( IJ4JLogger ),
                    typeof( IEnumerable<ITextToValue> ) ) 
                );

            _optionGenerators = GetTypeInfos<IOptionsGenerator>(
                PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired );
        }

        private List<TypeInfo> GetTypeInfos<T>( params PredefinedTypeTests[] predefinedTests )
            where T: class =>
            _exportedTypes.MeetRequirements<T>( predefinedTests )
                .Where( x => x.GetCustomAttribute<CommandLineOperatingSystemAttribute>() != null )
                .Select( x =>
                {
                    var attr = x.GetCustomAttribute<CommandLineCustomizationAttribute>();
                    var attrOS = x.GetCustomAttribute<CommandLineOperatingSystemAttribute>();

                    return new TypeInfo( attrOS?.OperatingSystem ?? string.Empty, attr!.Customization, attr!.Priority, x );
                } )
                .ToList();

        private List<TypeInfo> GetTypeInfos<T>(params ITypeTester[] tests)
            where T : class =>
            _exportedTypes.MeetRequirements<T>(tests)
                .Where(x => x.GetCustomAttribute<CommandLineOperatingSystemAttribute>() != null)
                .Select(x =>
                {
                    var attr = x.GetCustomAttribute<CommandLineCustomizationAttribute>();
                    var attrOS = x.GetCustomAttribute<CommandLineOperatingSystemAttribute>();

                    return new TypeInfo(attrOS?.OperatingSystem ?? string.Empty, attr!.Customization, attr!.Priority, x);
                })
                .ToList();

        public void AddOperatingSystemSynonym( string osName, string synonymOSName )
        {
            if( !_osSynonyms.ContainsKey( osName ) )
                _osSynonyms.Add( osName, new List<string>() );

            if( !_osSynonyms[ osName ].Any( x => x.Equals( synonymOSName, StringComparison.OrdinalIgnoreCase ) ) )
                _osSynonyms[ osName ].Add( synonymOSName );
        }

        private List<ITextToValue>? GetTextToValueConverters()
        {
            // create a list of instances and then filter it to eliminate
            // overridden types
            var rawList = CreateInstances<ITextToValue>( _textToValue, null, _loggerFactory?.Invoke() );

            if( rawList == null )
                return null;

            var retVal = new List<ITextToValue>();

            foreach( var group in rawList.GroupBy( x => x.TargetType ) )
            {
                retVal.Add( group
                    .OrderByDescending( x => x.Customization )
                    .ThenByDescending( x => x.Priority )
                    .First() );
            }

            return retVal;
        }

        private List<T>? CreateInstances<T>( List<TypeInfo> typeInfos, string? osName, params object?[] ctorParams )
        {
            // create a list of instances and then filter it to eliminate
            // overridden types
            List<T>? retVal = null;

            try
            {
                retVal = typeInfos.Select(x => (T)Activator.CreateInstance(typeof(T),ctorParams)!)
                    .ToList();
            }
            catch (Exception e)
            {
                if( string.IsNullOrEmpty(osName))
                    _logger?.Error<Type, string>(
                    "Could not create list of {0} objects. Exception was '{1}'",
                    typeof(T),
                    e.Message);
                else
                    _logger?.Error<Type, string>(
                        "Could not create list of {0} objects for operating system '{1}'. Exception was '{2}'",
                        typeof(T),
                        osName,
                        e.Message);

                return null;
            }

            return retVal;
        }

        public IAvailableTokens? GetAvailableTokens( string osName )
        {
            var conformingType = GetConformingTypes( _tokens, osName )
                .OrderByDescending( x => x.Customization )
                .ThenByDescending( x => x.Priority )
                .FirstOrDefault();

            return CreateInstance<IAvailableTokens>( conformingType, osName, _loggerFactory?.Invoke() );
        }

        public IMasterTextCollection? GetMasterTextCollection( string osName )
        {
            var conformingType = GetConformingTypes(_masterText, osName)
                .OrderByDescending(x => x.Customization)
                .ThenByDescending(x => x.Priority)
                .FirstOrDefault();

            return CreateInstance<IMasterTextCollection>(conformingType, osName, _loggerFactory?.Invoke());
        }

        public IBindabilityValidator? GetBindabilityValidator(string osName)
        {
            // create a list of instances and then filter it to eliminate
            // overridden types
            var converters = GetTextToValueConverters();
            if( converters == null )
            {
                _logger?.Error(
                    "Could not create IBindabilityValidator because there no ITextToValue converters are available" );
                return null;
            }

            var conformingTypes = GetConformingTypes( _bindabilityValidators, osName );

            var conformingType = conformingTypes.OrderByDescending( x => x.Customization )
                .ThenByDescending( x => x.Priority )
                .FirstOrDefault();

            if( conformingType == null )
            {
                _logger?.Error<string>( 
                    "No IBindabilityValidator implementation is available for operating system '{0}'",
                    osName );

                return null;
            }

            try
            {
                return (IBindabilityValidator) Activator.CreateInstance( typeof( IBindabilityValidator ),
                    new object?[] { converters, _loggerFactory?.Invoke() } )!;
            }
            catch( Exception e )
            {
                _logger?.Error<string, string>(
                    "Could not create instance of IBindabilityValidator for operating system '{0}'. Exception was '{1}'",
                    osName, 
                    e.Message );

                return null;
            }
        }

        public IOptionsGenerator? GetOptionsGenerator()
        {
            var conformingType = _optionGenerators
                .OrderByDescending(x => x.Customization)
                .ThenByDescending(x => x.Priority)
                .FirstOrDefault();

            return CreateInstance<IOptionsGenerator>( conformingType, null, _loggerFactory?.Invoke() );
        }

        private List<TypeInfo> GetConformingTypes( List<TypeInfo> typeInfoList, string osName )
        {
            var retVal = new List<TypeInfo>();

            retVal.AddRange(
                typeInfoList
                    .Where( x => x.OperatingSystem.Equals( osName, StringComparison.OrdinalIgnoreCase ) )
                    .ToList() );

            if( !_osSynonyms.ContainsKey( osName ) )
                return retVal;

            foreach( var synonym in _osSynonyms[ osName ] )
            {
                retVal.AddRange(
                    typeInfoList
                        .Where(x => x.OperatingSystem.Equals(synonym, StringComparison.OrdinalIgnoreCase))
                        .ToList());
            }

            return retVal;
        }

        private T? CreateInstance<T>( TypeInfo? typeInfo, string? osName, params object?[] ctorParameters )
        where T: class
        {
            if( typeInfo == null )
            {
                if( string.IsNullOrEmpty( osName ) )
                    _logger?.Error(
                        "Could not find an applicable {0} implementation",
                        typeof( T ) );
                else
                    _logger?.Error<Type, string>(
                        "Could not find an applicable {0} implementation for operating system '{1}'",
                        typeof( T ),
                        osName );

                return null;
            }

            try
            {
                return (T)Activator.CreateInstance(typeInfo.Type, ctorParameters)!;
            }
            catch (Exception e)
            {
                _logger?.Error<Type, string, string>(
                    "Could not create {0} object for operating system '{1}'. Exception was '{2}'",
                    typeof(T),
                    osName,
                    e.Message);

                return null;
            }
        }
    }
}
