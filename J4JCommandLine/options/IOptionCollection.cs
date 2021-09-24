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
using System.Linq.Expressions;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IOptionCollection : IEnumerable<IOption>
    {
        event EventHandler? Configured;
        bool IsConfigured { get; }
        void FinishConfiguration();

        ReadOnlyCollection<IOption> Options { get; }
        int Count { get; }
        //bool TargetsMultipleTypes { get; }

        List<string> SpuriousValues { get; }
        List<CommandLineArgument> UnknownKeys { get; }
        IOption? this[ string key ] { get; }
        void ClearValues();

        //void SetTypePrefix<TTarget>( string prefix )
        //    where TTarget : class, new();

        //string GetTypePrefix<TTarget>()
        //    where TTarget : class, new();

        //Option<TProp>? Add<TProp>( string contextPath );
        //IOption? Add( Type propType, string contextPath );

        Option<TProp>? Bind<TTarget, TProp>(
            Expression<Func<TTarget, TProp>> propertySelector,
            params string[] cmdLineKeys )
            where TTarget : class, new();

        bool CommandLineKeyInUse( string key );
        //bool UsesContextPath( string contextPath );

        bool KeysSpecified( params string[] keys );
        //void HelpDisplay( IHelpDisplay? displayHelp );
    }
}