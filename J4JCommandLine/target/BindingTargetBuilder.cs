using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public class BindingTargetBuilder
    {
        private readonly IAllocator _parser;
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly IConsoleOutput _consoleOutput;

        private StringComparison _textComp = StringComparison.OrdinalIgnoreCase;
        private bool _ignoreUnprocesssed = true;
        private string _progName;
        private string _description;
        private string[] _prefixes;
        private string[] _helpKeys;
        private string[] _enclosers;
        private char[] _quotes;

        public BindingTargetBuilder(
            IAllocator parser,
            IEnumerable<ITextConverter> converters,
            IConsoleOutput consoleOutput
        )
        {
            _parser = parser;
            _converters = converters;
            _consoleOutput = consoleOutput;
        }

        public BindingTargetBuilder CaseSensitivity( StringComparison textComp )
        {
            _textComp = textComp;
            return this;
        }

        public BindingTargetBuilder IgnoreUnprocessedUnkeyedParameters( bool ignore )
        {
            _ignoreUnprocesssed = ignore;
            return this;
        }

        public BindingTargetBuilder ProgramName( string programName )
        {
            _progName = programName;
            return this;
        }

        public BindingTargetBuilder Description( string description )
        {
            _description = description;
            return this;
        }

        public BindingTargetBuilder Prefixes( params string[] prefixes )
        {
            _prefixes = prefixes;
            return this;
        }

        public BindingTargetBuilder HelpKeys( params string[] helpKeys )
        {
            _helpKeys = helpKeys;
            return this;
        }

        public BindingTargetBuilder ValueEnclosers( params string[] enclosers )
        {
            _enclosers = enclosers;
            return this;
        }

        public BindingTargetBuilder Quotes( params char[] quoteChars )
        {
            _quotes = quoteChars;
            return this;
        }

        public BindingTarget<TValue>? Build<TValue>( TValue? value )
            where TValue : class
        {
            var errors = new CommandLineLogger( _textComp );

            var masterText = new MasterTextCollection( _textComp );

            masterText.AddRange(TextUsageType.Prefix, _prefixes);
            masterText.AddRange(TextUsageType.ValueEncloser, _enclosers);
            masterText.AddRange(TextUsageType.Quote, _quotes.Select(q => q.ToString()));

            if ( _helpKeys == null || _helpKeys.Length == 0 )
            {
                errors.LogError(ProcessingPhase.Initializing, $"No help keys defined");
                DisplayErrors(errors, masterText);

                return null;
            }

            masterText.AddRange( TextUsageType.HelpOptionKey, _helpKeys );

            if ( value == null && !typeof(TValue).HasPublicParameterlessConstructor() )
            {
                errors.LogError(ProcessingPhase.Initializing,$"{typeof(TValue)} does not have a public parameterless constructor");
                DisplayErrors(errors, masterText);

                return null;
            }

            value ??= Activator.CreateInstance<TValue>();

            if ( !_parser.Initialize( _textComp, errors, masterText ) )
            {
                DisplayErrors(errors, masterText);

                return null;
            }

            var retVal = new BindingTarget<TValue>()
            {
                Value = value,
                Allocator = _parser,
                Converters = _converters,
                TypeFactory = new TargetableTypeFactory(_converters),
                IgnoreUnkeyedParameters = _ignoreUnprocesssed,
                Options = new OptionCollection(masterText),
                Logger = errors,
                TextComparison = _textComp,
                MasterText = masterText,
                ConsoleOutput = _consoleOutput,
                ProgramName = _progName,
                Description = _description
            };

            if( !retVal.Initialize() )
            {
                errors.LogError(ProcessingPhase.Initializing, $"{retVal.GetType().Name} is not configured");
                DisplayErrors(errors, masterText);

                return null;
            }

            return retVal;
        }

        private void DisplayErrors( CommandLineLogger logger, MasterTextCollection masterText )
        {
            _consoleOutput.Initialize();

            if( !string.IsNullOrEmpty(_progName))
                _consoleOutput.AddLine(ConsoleSection.Header, _progName);

            if (!string.IsNullOrEmpty(_description))
                _consoleOutput.AddLine(ConsoleSection.Header, _description);

            foreach( var consolidatedError in logger.ConsolidateLogEvents(masterText) )
            {
                _consoleOutput.AddError( consolidatedError );
            }

            _consoleOutput.Display();
        }
    }
}