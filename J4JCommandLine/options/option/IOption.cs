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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IOption
    {
        bool IsInitialized { get; }
        IOptionCollection Container { get; }

        string? ContextPath { get; }

        ReadOnlyCollection<string> Keys { get; }
        string? CommandLineKeyProvided { get; set; }

        OptionStyle Style { get; }

        ReadOnlyCollection<string> Values { get; }
        int MaxValues { get; }
        int NumValuesAllocated { get; }
        bool ValuesSatisfied { get; }
        bool GetValue( out object? result );

        bool Required { get; }

        string? Description { get; }
        IOption AddCommandLineKey( string cmdLineKey );
        IOption AddCommandLineKeys( IEnumerable<string> cmdLineKeys );
        IOption SetStyle( OptionStyle style );
        void AddValue( string value );
        void AddValues( IEnumerable<string> values );
        void ClearValues();
        IOption IsRequired();
        IOption IsOptional();
        IOption SetDescription( string description );

        string? GetDefaultValue();
    }

    public interface IOption<T> : IOption
    {
        T? DefaultValue { get; }
        IOption<T> SetDefaultValue( T? value );
    }

    public interface ITypeBoundOption : IOption
    {
        Type ContainerType { get; }
    }
}