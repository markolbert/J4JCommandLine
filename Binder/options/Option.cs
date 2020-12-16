using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class Option
    {
        private readonly List<string> _cmdLineKeys = new List<string>();
        private readonly List<string> _allocatedValues = new List<string>();
        private readonly MasterTextCollection _masterText;

        private int _allowedNumValues;

        internal Option( 
            Options container, 
            IContextKey contextKey,
            MasterTextCollection masterText 
            )
        {
            Container = container;
            ContextKey = contextKey;
            _masterText = masterText;

            if( Container.UsesContextPath( ContextPath! ) )
                throw new ArgumentException( $"Duplicate context key path '{ContextPath}'" );
        }

        // the collection of Options used by the parsing activity
        public Options Container { get; }

        public IContextKey? ContextKey { get; }

        public List<IContextKey>? ContextPath
        {
            get
            {
                if( Parent == null || ContextKey == null )
                {
                    if( ContextKey == null )
                        return null;

                    return new List<IContextKey> { ContextKey };
                }

                var retVal = Parent.ContextPath!;

                retVal.Add( ContextKey );

                return retVal;
            }
        }

        public Option? Parent { get; private set; }

        public ReadOnlyCollection<string> Keys => _cmdLineKeys.AsReadOnly();

        // the first key defined for an option, sorted alphabetically (Options can define multiple keys
        // but they must be unique within the scope of all Options)
        public string FirstKey => _cmdLineKeys.OrderBy( k => k ).First();

        public string? CommandLineKeyProvided { get; set; }

        public bool WasAssignedValue
        {
            get
            {
                if( string.IsNullOrEmpty( CommandLineKeyProvided ) )
                    return false;

                var numValuesAlloc = _allocatedValues.Count;

                return Style switch
                {
                    OptionStyle.Switch => numValuesAlloc == 0,
                    OptionStyle.SingleValued => numValuesAlloc == 1,
                    OptionStyle.Collection => numValuesAlloc > 0,
                    _ => throw new InvalidEnumArgumentException($"Unsupported OptionStyle '{Style}'")
                };
            }
        }
        
        public ReadOnlyCollection<string> CommandLineValues => _allocatedValues.AsReadOnly();

        public OptionStyle Style { get; private set; }

        public bool Required { get; private set; }
        public string? Description { get; private set; }

        internal void AddAllocatedValue( string value ) => _allocatedValues.Add( value );

        public Option AddCommandLineKey(string cmdLineKey)
        {
            if( !Container.UsesCommandLineKey( cmdLineKey ) )
            {
                _cmdLineKeys.Add(cmdLineKey);
                _masterText.Add( TextUsageType.OptionKey, cmdLineKey );
            }

            return this;
        }

        public Option AddCommandLineKeys(params string[] cmdLineKeys)
        {
            foreach( var cmdLineKey in cmdLineKeys )
            {
                AddCommandLineKey( cmdLineKey );
            }

            return this;
        }

        public Option SetStyle( OptionStyle style )
        {
            _allowedNumValues = style switch
            {
                OptionStyle.Collection => Int32.MaxValue,
                OptionStyle.SingleValued => 1,
                OptionStyle.Switch => 0,
                _ => throw new InvalidEnumArgumentException( $"Unsupported OptionStyle '{style}'" )
            };

            return this;
        }

        public Option IsRequired()
        {
            Required = true;
            return this;
        }

        public Option IsOptional()
        {
            Required = false;
            return this;
        }

        public Option SetDescription(string description)
        {
            Description = description;
            return this;
        }

        public Option ChildOf(Option parent)
        {
            if (parent != this)
                Parent = parent;

            return this;
        }
    }
}