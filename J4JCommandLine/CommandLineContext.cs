using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class CommandLineContext
    {
        private readonly Dictionary<string, IBindingTarget> _bindingTargets = new Dictionary<string, IBindingTarget>();
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly Func<IJ4JLogger>? _loggerFactory;
        private readonly IHelpErrorProcessor _helpErrorProcessor;

        public CommandLineContext(
            ICommandLineTextParser textParser,
            IEnumerable<ITextConverter> converters,
            IParsingConfiguration parsingConfig,
            IHelpErrorProcessor helpErrorProcessor,
            Func<IJ4JLogger>? loggerFactory
        )
        {
            TextParser = textParser;
            _converters = converters;
            ParsingConfiguration = parsingConfig;
            _helpErrorProcessor = helpErrorProcessor;
            _loggerFactory = loggerFactory;

            Options = new OptionCollection( parsingConfig, _loggerFactory?.Invoke() );
            Errors = new CommandLineErrors(ParsingConfiguration);
        }

        public ICommandLineTextParser TextParser { get; }
        public IParsingConfiguration ParsingConfiguration { get; }
        public string? Description { get; set; }
        public string? ProgramName { get; set; }
        public IOptionCollection Options { get; }
        public CommandLineErrors Errors { get; }

        public ReadOnlyDictionary<string, IBindingTarget> BindingTargets =>
            new ReadOnlyDictionary<string, IBindingTarget>( _bindingTargets );

        public IBindingTarget<TTarget> AddBindingTarget<TTarget>( TTarget? value, string targetID )
            where TTarget : class
        {
            if( string.IsNullOrEmpty( targetID ) )
                throw new NullReferenceException( $"{nameof(targetID)} cannot be null or empty" );

            if( _bindingTargets.ContainsKey( targetID ) )
                throw new IndexOutOfRangeException( $"Duplicate binding target key '{targetID}'" );

            var options = new OptionCollection( ParsingConfiguration, _loggerFactory?.Invoke() );

            var retVal = value == null
                ? new BindingTarget<TTarget>( targetID, _converters, Options, ParsingConfiguration, Errors,
                    _loggerFactory )
                : new BindingTarget<TTarget>( targetID, value, _converters, Options, ParsingConfiguration, Errors,
                    _loggerFactory );

            _bindingTargets.Add( targetID, retVal );

            return retVal;
        }

        public MappingResults Parse( string[] args )
        {
            var retVal = MappingResults.Success;

            // parse the arguments into a collection of arguments keyed by the option key
            // note that there can be multiple arguments associated with any option key
            var results = TextParser.Parse( args );

            foreach( var kvp in _bindingTargets )
            {
                retVal |= kvp.Value.MapParseResults( results );
            }

            if( results.Any(
                pr => ParsingConfiguration.HelpKeys
                    .Any( k => string.Equals( k, pr.Key, ParsingConfiguration.TextComparison ) ) ) )
                retVal |= MappingResults.HelpRequested;

            _helpErrorProcessor.Display( retVal, this );

            Errors.Clear();

            return retVal;
        }
    }
}