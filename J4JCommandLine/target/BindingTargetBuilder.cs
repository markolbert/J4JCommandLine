using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace J4JSoftware.CommandLine
{
    public class BindingTargetBuilder
    {
        private readonly ICommandLineParser _parser;
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
            ICommandLineParser parser,
            IEnumerable<ITextConverter> converters,
            IHelpErrorProcessor helpErrorProcessor
        )
        {
            _parser = parser;
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

            if( !typeof(TValue).HasPublicParameterlessConstructor() )
            {
                errors.AddError(null, null, $"{typeof(TValue)} does not have a public parameterless constructor");
                return false;
            }

            value ??= Activator.CreateInstance<TValue>();

            if ( !IsValid )
            {
                errors.AddError( null, null, $"Invalid {nameof(BindingTargetBuilder)} configuration" );
                return false;
            }

            if( !_parser.Initialize( _caseSensitivity, errors, _prefixes, _enclosers, _quotes ) )
                return false;

            if( !_helpErrorProcessor.Initialize( _caseSensitivity, errors, _parser.Prefixer, _helpKeys ) )
                return false;

            result = new BindingTarget<TValue>( value, 
                _parser, 
                _converters, 
                _helpErrorProcessor, 
                _caseSensitivity,
                errors )
            {
                ProgramName = _progName, 
                Description = _description
            };

            result.Initialize();

            return true;
        }
    }
}