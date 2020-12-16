using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace J4JSoftware.CommandLine
{
    public class Option
    {
        private readonly List<string> _cmdLineKeys = new List<string>();
        private readonly List<string> _allocatedValues = new List<string>();
        private readonly MasterTextCollection _masterText;

        internal Option( 
            OptionsBase container, 
            string contextPath,
            MasterTextCollection masterText 
            )
        {
            Container = container;
            ContextPath = contextPath;
            _masterText = masterText;

            if( Container.UsesContextPath( ContextPath! ) )
                throw new ArgumentException( $"Duplicate context key path '{ContextPath}'" );
        }

        // the collection of Options used by the parsing activity
        public OptionsBase Container { get; }

        public string? ContextPath { get; }

        public ReadOnlyCollection<string> Keys => _cmdLineKeys.AsReadOnly();

        public string? CommandLineKeyProvided { get; set; }

        public int MaxValues =>
            Style switch
            {
                OptionStyle.Collection => int.MaxValue,
                OptionStyle.SingleValued => 1,
                OptionStyle.Switch => 0,
                _ => throw new InvalidEnumArgumentException( $"Unsupported OptionStyle '{Style}'" )
            };

        public int NumValuesAllocated => _allocatedValues.Count;

        public bool ValuesSatisfied
        {
            get
            {
                if (string.IsNullOrEmpty(CommandLineKeyProvided))
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
            Style = style;
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
    }
}