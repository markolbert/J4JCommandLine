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
            Type Type,
            object[]? ConstructorParameters
        );

        private static Type[] _supportedTypes = new[]
        {
            typeof( ITextToValue ),
            typeof( IAvailableTokens ),
            typeof( IMasterTextCollection ),
            typeof( IBindabilityValidator ),
            typeof( IOptionsGenerator ),
        };

        private readonly List<Type> _exportedTypes;
        private readonly Dictionary<string, List<string>> _osSynonyms = new(StringComparer.OrdinalIgnoreCase);
        private readonly IJ4JLoggerFactory? _loggerFactory;
        private readonly IJ4JLogger? _logger;

        private readonly List<TypeInfo> _textToValue;
        private readonly List<TypeInfo> _tokens;
        private readonly List<TypeInfo> _masterText;
        private readonly List<TypeInfo> _bindabilityValidators;
        private readonly List<TypeInfo> _optionGenerators;

        public J4JCommandLineFactory(
            IEnumerable<Assembly>? assemblies = null,
            IJ4JLoggerFactory? loggerFactory = null
        )
        {
            assemblies ??= Enumerable.Empty<Assembly>();
            var assemblyList = assemblies.ToList();
            assemblyList.Add( GetType().Assembly );
            assemblyList = assemblyList.Distinct().ToList();

            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.CreateLogger( GetType() );
            _logger?.SetLoggedType( GetType() );

            _exportedTypes = assemblyList.SelectMany( x => x.ExportedTypes )
                .Where( x => _supportedTypes.Any( y => y.IsAssignableFrom( x ) )
                             && x.GetConstructors().Any() )
                .ToList();

            _textToValue = GetTextToValueConverterTypes();

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
                .Where( x => x.GetCustomAttribute<CommandLineCustomizationAttribute>() != null )
                .Select( x =>
                {
                    var attr = x.GetCustomAttribute<CommandLineCustomizationAttribute>();
                    var attrOS = x.GetCustomAttribute<CommandLineOperatingSystemAttribute>();

                    return new TypeInfo( attrOS?.OperatingSystem ?? OSNames.Universal, attr!.Customization, attr!.Priority, x, null );
                } )
                .ToList();

        private List<TypeInfo> GetTextToValueConverterTypes()
        {
            // we need to include the built-in converters, which derive from a generic type
            var retVal = GetTypeInfos<ITextToValue>( PredefinedTypeTests.NonAbstract,
                PredefinedTypeTests.OnlyJ4JLoggerRequired, 
                PredefinedTypeTests.NonGeneric );

            foreach (var convMethod in typeof(Convert)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m =>
                {
                    var parameters = m.GetParameters();

                    return parameters.Length == 1 && !typeof(string).IsAssignableFrom(parameters[0].ParameterType);
                }))
            {
                retVal.Add( new TypeInfo(
                    OSNames.Universal,
                    Customization.BuiltIn,
                    Int32.MinValue,
                    typeof( BuiltInTextToValue<> ).MakeGenericType( convMethod.ReturnType ),
                    new object[] { convMethod } )
                );
            }

            return retVal;
        }

        private List<TypeInfo> GetTypeInfos<T>(params ITypeTester[] tests)
            where T : class =>
            _exportedTypes.MeetRequirements<T>(tests)
                .Where(x => x.GetCustomAttribute<CommandLineOperatingSystemAttribute>() != null)
                .Select(x =>
                {
                    var attr = x.GetCustomAttribute<CommandLineCustomizationAttribute>();
                    var attrOS = x.GetCustomAttribute<CommandLineOperatingSystemAttribute>();

                    return new TypeInfo(attrOS?.OperatingSystem ?? string.Empty, attr!.Customization, attr!.Priority, x, null);
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
            var rawList = CreateTextToValueConverters();

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

        private List<ITextToValue>? CreateTextToValueConverters()
        {
            // create a list of instances and then filter it to eliminate
            // overridden types
            List<ITextToValue>? retVal = new List<ITextToValue>();

            foreach( var typeInfo in _textToValue )
            {
                try
                {
                    var ctorParams = new List<object?>();
                    ctorParams.AddRange( typeInfo.ConstructorParameters! );
                    ctorParams.Add( _loggerFactory?.CreateLogger<ITypeTester>() );

                    retVal.Add( (ITextToValue) Activator.CreateInstance( typeInfo.Type, ctorParams.ToArray() )! );
                }
                catch( Exception e )
                {
                    _logger?.Error<string>(
                        "Could not create list of ITextToValue objects. Exception was '{0}'",
                        e.Message );

                    return null;
                }

            }
            //retVal = typeInfos.Select(x => (T)Activator.CreateInstance(typeof(T),ctorParams)!)
            //    .ToList();

            return retVal;
        }

        public IParser? GetParser( string osName, IDisplayHelp? displayHelp = null, params ICleanupTokens[] cleanupTokens )
        {
            displayHelp ??= new DefaultDisplayHelp( _loggerFactory?.CreateLogger<IDisplayHelp>() );

            var mtCollection = GetMasterTextCollection( osName );
            if( mtCollection == null )
                return null;

            var bindabilityValidator = GetBindabilityValidator( osName );
            if( bindabilityValidator == null )
                return null;

            var options = new OptionCollection( mtCollection, 
                bindabilityValidator, 
                displayHelp,
                _loggerFactory?.CreateLogger<IOptionCollection>() );

            var generator = GetOptionsGenerator( mtCollection.TextComparison, options );
            if( generator == null )
                return null;

            var tokens = GetAvailableTokens( osName );
            if( tokens == null )
                return null;

            var tokenizer = new Tokenizer( tokens, _loggerFactory );

            return new Parser( options,
                new ParsingTable( generator, _loggerFactory?.CreateLogger<IParsingTable>() ),
                tokenizer,
                _loggerFactory?.CreateLogger<IParser>() );
        }

        public IAvailableTokens? GetAvailableTokens( string osName )
        {
            var conformingType = GetConformingTypes( _tokens, osName )
                .OrderByDescending( x => x.Customization )
                .ThenByDescending( x => x.Priority )
                .FirstOrDefault();

            return CreateInstance<IAvailableTokens>( conformingType, osName,
                _loggerFactory?.CreateLogger<IAvailableTokens>() );
        }

        public IMasterTextCollection? GetMasterTextCollection( string osName )
        {
            var conformingType = GetConformingTypes(_masterText, osName)
                .OrderByDescending(x => x.Customization)
                .ThenByDescending(x => x.Priority)
                .FirstOrDefault();

            return CreateInstance<IMasterTextCollection>( conformingType, osName,
                _loggerFactory?.CreateLogger<IMasterTextCollection>() );
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
                return (IBindabilityValidator) Activator.CreateInstance( conformingType.Type,
                    new object?[] { converters, _loggerFactory?.CreateLogger<IBindabilityValidator>() } )!;
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

        public IOptionsGenerator? GetOptionsGenerator( StringComparison textComparison, IOptionCollection options )
        {
            var conformingType = _optionGenerators
                .OrderByDescending( x => x.Customization )
                .ThenByDescending( x => x.Priority )
                .FirstOrDefault();

            var retVal = CreateInstance<IOptionsGenerator>( conformingType, null,
                _loggerFactory?.CreateLogger<IOptionsGenerator>() );

            retVal?.Initialize( textComparison, options );

            return retVal;
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
