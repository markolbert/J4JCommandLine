using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class BindingTargetBuilder
    {
        private readonly ICommandLineParser _parser;
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly IConsoleOutput _consoleOutput;

        private StringComparison _caseSensitivity = StringComparison.OrdinalIgnoreCase;
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
            _caseSensitivity = textComp;
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

        public bool Build<TValue>( TValue? value, out BindingTarget<TValue>? result, out CommandLineErrors errors )
            where TValue : class
        {
            errors = new CommandLineErrors( _caseSensitivity );
            result = null;

            var masterText = new MasterTextCollection( _caseSensitivity );

            if( _helpKeys == null || _helpKeys.Length == 0 )
            {
                errors.AddError(null, null, $"No help keys defined");
                DisplayErrors(errors);

                return false;
            }

            masterText.AddRange( TextUsageType.HelpOptionKey, _helpKeys );
            masterText.AddRange( TextUsageType.Prefix, _prefixes );
            masterText.AddRange( TextUsageType.ValueEncloser, _enclosers );
            masterText.AddRange( TextUsageType.Quote, _quotes.Select( q => q.ToString() ) );

            if ( !typeof(TValue).HasPublicParameterlessConstructor() )
            {
                errors.AddError(null, null, $"{typeof(TValue)} does not have a public parameterless constructor");
                DisplayErrors(errors);

                return false;
            }

            value ??= Activator.CreateInstance<TValue>();

            if ( !_parser.Initialize( _caseSensitivity, errors, masterText ) )
            {
                DisplayErrors(errors);

                return false;
            }

            result = new BindingTarget<TValue>( value,
                _parser,
                _converters,
                _caseSensitivity,
                errors,
                masterText,
                _consoleOutput )
            {
                ProgramName = _progName,
                Description = _description
            };

            result.Initialize();

            return true;
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