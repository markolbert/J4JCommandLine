using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public class BindingTargetBuilder
    {
        private readonly ICommandLineTextParser _textParser;
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly IHelpErrorProcessor _helpErrorProcessor;

        private StringComparison _caseSensitivity = StringComparison.OrdinalIgnoreCase;
        private string _progName;
        private string _description;
        private string[] _prefixes;
        private string[] _helpKeys;
        private string[] _enclosers;
        private char[] _quotes;

        public BindingTargetBuilder(
            ICommandLineTextParser textParser,
            IEnumerable<ITextConverter> converters,
            IHelpErrorProcessor helpErrorProcessor
        )
        {
            _textParser = textParser;
            _converters = converters;
            _helpErrorProcessor = helpErrorProcessor;
        }

        public bool IsValid => _prefixes?.Length > 0
                               && _helpKeys?.Length > 0;

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

            if( !IsValid )
            {
                errors.AddError( null, null, $"Invalid {nameof(BindingTargetBuilder)} configuration" );
                return false;
            }

            if( !_textParser.Initialize( _caseSensitivity, _prefixes, _enclosers, _quotes ) )
            {
                errors.AddError( null, null, $"Failed to initialize {nameof(ICommandLineTextParser)}" );
                return false;
            }

            if( !_helpErrorProcessor.Initialize( _caseSensitivity, _textParser.Prefixer, _helpKeys ) )
            {
                errors.AddError(null, null, $"Failed to initialize {nameof(IHelpErrorProcessor)}");
                return false;
            }

            result = value == null
                ? new BindingTarget<TValue>( _textParser, _converters, _helpErrorProcessor, _caseSensitivity, errors )
                : new BindingTarget<TValue>( value, _textParser, _converters, _helpErrorProcessor, _caseSensitivity, errors );

            result.ProgramName = _progName;
            result.Description = _description;

            result.Initialize();

            return true;
        }
    }
}