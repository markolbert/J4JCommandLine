using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class Option
    {
        private readonly List<string> _cmdLineKeys = new List<string>();
        private readonly List<string> _allocatedValues = new List<string>();

        internal Option( Options container, IContextKey contextKey )
        {
            Container = container;
            ContextKey = contextKey;

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

        public string? CommandLineKeyUsed { get; set; }
        public ReadOnlyCollection<string> CommandLineValues => _allocatedValues.AsReadOnly();

        public int AllowedNumberOfValues { get; private set; }
        public bool Required { get; private set; }
        public string? Description { get; private set; }

        internal void AddAllocatedValue( string value ) => _allocatedValues.Add( value );

        public Option AddCommandLineKey(string cmdLineKey)
        {
            if (!Container.UsesCommandLineKey(cmdLineKey))
                _cmdLineKeys.Add(cmdLineKey);

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

        public Option IsSwitch()
        {
            AllowedNumberOfValues = 0;
            return this;
        }

        public Option IsCollection()
        {
            AllowedNumberOfValues = Int32.MaxValue;
            return this;
        }

        public Option IsSingleValue()
        {
            AllowedNumberOfValues = 1;
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