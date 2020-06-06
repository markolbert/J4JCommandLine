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

        public BindingTarget<TValue>? Build<TValue>( TValue? value = null )
            where TValue : class
        {
            if( !IsValid )
                throw new NullReferenceException();
                //return null;

            _textParser.Initialize(_caseSensitivity, _prefixes, _enclosers, _quotes);
            _helpErrorProcessor.Initialize( _caseSensitivity, _textParser.Prefixer, _helpKeys );

            if( !_helpErrorProcessor.IsInitialized
                || !_textParser.IsInitialized )
                return null;

            var retVal = value == null
                ? new BindingTarget<TValue>( _textParser, _converters, _helpErrorProcessor, _caseSensitivity )
                : new BindingTarget<TValue>( value, _textParser, _converters, _helpErrorProcessor, _caseSensitivity );

            retVal.ProgramName = _progName;
            retVal.Description = _description;

            retVal.Initialize();

            return retVal;
        }
    }
}