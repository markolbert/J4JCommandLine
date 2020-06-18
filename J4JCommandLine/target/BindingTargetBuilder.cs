using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Linq;
using System.Security.Cryptography;

namespace J4JSoftware.CommandLine
{
    public partial class BindingTargetBuilder
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
            var config = new Configuration();

            if( !config.Initialize( this, value ) )
            {
                DisplayErrors( config.Logger, config.MasterText );
                return null;
            }

            value ??= Activator.CreateInstance<TValue>();

            if ( !_parser.Initialize( _textComp, config.Logger, config.MasterText) )
            {
                DisplayErrors(config.Logger, config.MasterText);

                return null;
            }

            var retVal = new BindingTarget<TValue>()
            {
                Value = value,
                Allocator = _parser,
                Converters = _converters,
                TypeFactory = new TargetableTypeFactory(_converters),
                IgnoreUnkeyedParameters = _ignoreUnprocesssed,
                Options = new OptionCollection(config.MasterText),
                Logger = config.Logger,
                TextComparison = _textComp,
                MasterText = config.MasterText,
                ConsoleOutput = _consoleOutput,
                ProgramName = _progName,
                Description = _description
            };

            if( !retVal.Initialize() )
            {
                config.Logger.LogError(ProcessingPhase.Initializing, $"{retVal.GetType().Name} is not configured");
                DisplayErrors(config.Logger, config.MasterText);

                return null;
            }

            return retVal;
        }

        public BindingTarget<TValue>? AutoBind<TValue>()
            where TValue : class
        {
            var config = new Configuration();

            if (!config.Initialize<TValue>(this, null))
            {
                DisplayErrors(config.Logger, config.MasterText);
                return null;
            }

            if (!_parser.Initialize(_textComp, config.Logger, config.MasterText))
            {
                DisplayErrors(config.Logger, config.MasterText);
                return null;
            }

            var boundProps = new List<OptionConfiguration>();
            var propStack = new Stack<PropertyInfo>();

            FindBoundProperties(typeof(TValue), ref propStack, ref boundProps);

            if( boundProps.Count( bp => bp.Unkeyed ) > 1 )
            {
                config.Logger.LogError(ProcessingPhase.Initializing, "Multiple unkeyed Options specified");

                DisplayErrors(config.Logger, config.MasterText);
                return null;
            }

            var value = Activator.CreateInstance<TValue>();

            var retVal = new BindingTarget<TValue>()
            {
                Value = value,
                Allocator = _parser,
                Converters = _converters,
                TypeFactory = new TargetableTypeFactory(_converters),
                IgnoreUnkeyedParameters = _ignoreUnprocesssed,
                Options = new OptionCollection(config.MasterText),
                Logger = config.Logger,
                TextComparison = _textComp,
                MasterText = config.MasterText,
                ConsoleOutput = _consoleOutput,
                ProgramName = _progName,
                Description = _description
            };

            if (!retVal.Initialize())
            {
                config.Logger.LogError(ProcessingPhase.Initializing, $"{retVal.GetType().Name} is not configured");
                DisplayErrors(config.Logger, config.MasterText);

                return null;
            }

            foreach( var boundProp in boundProps )
            {
                retVal.Bind( boundProp );
            }

            if( config.Logger.Count > 0 )
            {
                config.Logger.LogError(ProcessingPhase.Initializing, $"{retVal.GetType().Name} is not configured");
                DisplayErrors(config.Logger, config.MasterText);

                return null;
            }

            return retVal;
        }

        private void FindBoundProperties(
            Type toScan,
            ref Stack<PropertyInfo> propertyStack,
            ref List<OptionConfiguration> boundProps )
        {
            // search all public readable/writeable properties looking for OptionKeysAttributes
            foreach( var propInfo in toScan.GetProperties(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public ) )
            {
                var keysAttr = propInfo.GetCustomAttribute<OptionKeysAttribute>();

                if( keysAttr == null || !propInfo.CanRead || !propInfo.CanWrite )
                    continue;

                // found an OptionKeysAttribute so create a new OptionConfiguration object
                boundProps.Add( new OptionConfiguration( propInfo, propertyStack, keysAttr.Keys ) );

                // recurse
                propertyStack.Push( propInfo );
                FindBoundProperties( propInfo.PropertyType, ref propertyStack, ref boundProps );
            }

            if( propertyStack.Count > 0 )
                propertyStack.Pop();
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