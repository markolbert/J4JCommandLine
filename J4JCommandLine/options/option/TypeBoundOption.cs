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

namespace J4JSoftware.Configuration.CommandLine
{
    public class TypeBoundOption<TContainer, TProp> : Option<TProp>, ITypeBoundOption
        where TContainer : class, new()
    {
        internal TypeBoundOption(
            OptionCollection container,
            string typeRelativeContextPath,
            IBindabilityValidator propValidator,
            ITextConverters converters
        )
            : base( container, typeRelativeContextPath, propValidator, converters )
        {
        }

        public Type ContainerType => typeof(TContainer);

        //public override string? ContextPath
        //{
        //    get
        //    {
        //        if( base.ContextPath == null )
        //            return null;

        //        return $"{GetTypePrefix<TContainer>()}{base.ContextPath}";
        //    }
        //}

        //private string GetTypePrefix<TTarget>()
        //    where TTarget : class, new()
        //{
        //    var type = typeof(TTarget);

        //    if (Container.TypePrefixes.ContainsKey(type))
        //        return $"{Container.TypePrefixes[type]}:";

        //    return Container.TargetsMultipleTypes ? $"{type.Name}:" : string.Empty;
        //}
    }
}