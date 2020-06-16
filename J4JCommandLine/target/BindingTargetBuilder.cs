using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public class BindingTargetBuilder
    {
        private readonly ICommandLineParser _parser;
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
            ICommandLineParser parser,
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
            var errors = new CommandLineErrors( _textComp );

            var masterText = new MasterTextCollection( _textComp );

            if( _helpKeys == null || _helpKeys.Length == 0 )
            {
                errors.AddError(null, null, $"No help keys defined");
                DisplayErrors(errors);

                return null;
            }

            masterText.AddRange( TextUsageType.HelpOptionKey, _helpKeys );
            masterText.AddRange( TextUsageType.Prefix, _prefixes );
            masterText.AddRange( TextUsageType.ValueEncloser, _enclosers );
            masterText.AddRange( TextUsageType.Quote, _quotes.Select( q => q.ToString() ) );

            if ( value == null && !typeof(TValue).HasPublicParameterlessConstructor() )
            {
                errors.AddError(null, null, $"{typeof(TValue)} does not have a public parameterless constructor");
                DisplayErrors(errors);

                return null;
            }

            value ??= Activator.CreateInstance<TValue>();

            if ( !_parser.Initialize( _textComp, errors, masterText ) )
            {
                DisplayErrors(errors);

                return null;
            }

            var retVal = new BindingTarget<TValue>()
            {
                Value = value,
                Parser = _parser,
                Converters = _converters,
                TypeFactory = new TargetableTypeFactory(_converters),
                IgnoreUnkeyedParameters = _ignoreUnprocesssed,
                Options = new OptionCollection(masterText),
                Errors = errors,
                TextComparison = _textComp,
                MasterText = masterText,
                ConsoleOutput = _consoleOutput,
                ProgramName = _progName,
                Description = _description
            };

            if( !retVal.Initialize() )
            {
                errors.AddError(null, null, $"{retVal.GetType().Name} is not configured");
                DisplayErrors(errors);

                return null;
            }

            return retVal;
        }

        private void DisplayErrors( CommandLineErrors errors )
        {
            _consoleOutput.Initialize();

            if( !string.IsNullOrEmpty(_progName))
                _consoleOutput.AddLine(ConsoleSection.Header, _progName);

            if (!string.IsNullOrEmpty(_description))
                _consoleOutput.AddLine(ConsoleSection.Header, _description);

            foreach( var error in errors )
            {
                _consoleOutput.AddError( error.Errors );
            }

            _consoleOutput.Display();
        }
    }
}