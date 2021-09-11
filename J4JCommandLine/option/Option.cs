#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace J4JSoftware.Configuration.CommandLine
{
    public class Option<T> : IOption<T>
    {
        private readonly List<string> _cmdLineKeys = new();
        private readonly IMasterTextCollection _masterText;
        private readonly IBindabilityValidator _propValidator;
        private readonly List<string> _values = new();

        internal Option(
            IOptionCollection container,
            string contextPath,
            IMasterTextCollection masterText,
            IBindabilityValidator propValidator
        )
        {
            Container = container;
            ContextPath = contextPath;
            _masterText = masterText;
            _propValidator = propValidator;
        }

        public bool IsInitialized => !string.IsNullOrEmpty( ContextPath ) && _cmdLineKeys.Count > 0;
        public IOptionCollection Container { get; }

        public virtual string? ContextPath { get; }

        public ReadOnlyCollection<string> Keys => _cmdLineKeys.AsReadOnly();

        public IOption AddCommandLineKey( string cmdLineKey )
        {
            if( !Container.UsesCommandLineKey( cmdLineKey ) )
            {
                _cmdLineKeys.Add( cmdLineKey );
                _masterText.Add( TextUsageType.OptionKey, cmdLineKey );
            }

            return this;
        }

        public IOption AddCommandLineKeys( IEnumerable<string> cmdLineKeys )
        {
            foreach( var cmdLineKey in cmdLineKeys ) AddCommandLineKey( cmdLineKey );

            return this;
        }

        public string? CommandLineKeyProvided { get; set; }

        public OptionStyle Style { get; private set; } = OptionStyle.Undefined;

        public IOption SetStyle( OptionStyle style )
        {
            Style = style;
            return this;
        }

        public ReadOnlyCollection<string> Values => _values.AsReadOnly();

        public void ClearValues()
        {
            _values.Clear();
        }

        public void AddValue( string value )
        {
            _values.Add( value );
        }

        public void AddValues( IEnumerable<string> values )
        {
            _values.AddRange( values );
        }

        public int MaxValues =>
            Style switch
            {
                OptionStyle.Collection => int.MaxValue,
                OptionStyle.SingleValued => 1,
                OptionStyle.ConcatenatedSingleValue => int.MaxValue,
                OptionStyle.Switch => 0,
                _ => throw new InvalidEnumArgumentException( $"Unsupported OptionStyle '{Style}'" )
            };

        public int NumValuesAllocated => _values.Count;

        public bool ValuesSatisfied
        {
            get
            {
                if( string.IsNullOrEmpty( CommandLineKeyProvided ) )
                    return false;

                var numValuesAlloc = _values.Count;

                return Style switch
                {
                    OptionStyle.Switch => numValuesAlloc == 0,
                    OptionStyle.SingleValued => numValuesAlloc == 1,
                    OptionStyle.Collection => numValuesAlloc > 0,
                    OptionStyle.ConcatenatedSingleValue => numValuesAlloc > 0,
                    _ => throw new InvalidEnumArgumentException( $"Unsupported OptionStyle '{Style}'" )
                };
            }
        }

        public bool GetValue( out object? result )
        {
            result = default(T);
            
            if( !ValuesSatisfied )
                return false;

            if( Style != OptionStyle.Switch )
                return _propValidator.Convert( typeof(T), _values, out result );

            result = !string.IsNullOrEmpty( CommandLineKeyProvided );
            
            return true;
        }

        public bool Required { get; private set; }

        public IOption IsRequired()
        {
            Required = true;
            return this;
        }

        public IOption IsOptional()
        {
            Required = false;
            return this;
        }

        public string? Description { get; private set; }

        public IOption SetDescription( string description )
        {
            Description = description;
            return this;
        }

        public T? DefaultValue { get; private set; }

        public IOption<T> SetDefaultValue( T? value )
        {
            DefaultValue = value;
            return this;
        }

        string? IOption.GetDefaultValue()
        {
            return DefaultValue?.ToString();
        }
    }
}