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
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    // a class which keeps track of every text element (e.g., prefix, value encloser, quote character,
    // option key) used in the framework and ensures they are all used uniquely.
    public class MasterTextCollection : CustomizedEntity, IMasterTextCollection
    {
        private readonly List<TextUsage> _items = new();
        private readonly IJ4JLogger? _logger;

        protected MasterTextCollection(
            IJ4JLogger? logger = null
        )
            : base( false )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public void Initialize() => _items.Clear();

        // gets a list of all the items for the specified type of usage
        public List<string> this[ TextUsageType usage ] =>
            _items.Where( x => x.Usage == usage )
                .Select( x => x.Text )
                .ToList();

        // indicates whether the supplied text exists in the collection
        public bool Contains( string text )
        {
            return _items.Any( x => string.Equals( x.Text, text, TextComparison ) );
        }

        // indicates whether the supplied text exists in the collection for the specified usage
        public bool Contains( string text, TextUsageType usage )
        {
            return _items.Any( x => x.Usage == usage && string.Equals( x.Text, text, TextComparison ) );
        }

        public TextUsageType GetTextUsageType( string toCheck )
        {
            var item = _items.FirstOrDefault( x => x.Text.Equals( toCheck, TextComparison ) );

            return item?.Usage ?? TextUsageType.Undefined;
        }

        // adds an item to the collection
        public bool Add( TextUsageType usage, string item )
        {
            if( Contains( item ) )
            {
                _logger?.Information<TextUsageType, string>( "Duplicate {0} '{1}'", usage, item );
                return false;
            }

            if( usage == TextUsageType.Undefined )
            {
                _logger?.Information<TextUsageType, string>( "Cannot add {0} items ({1})", usage, item );
                return false;
            }

            _items.Add( new TextUsage( item, usage ) );

            return true;
        }

        // adds a range of text elements to the collection
        protected bool AddRange( TextUsageType usage, params string[] items )
        {
            return AddRange( usage, items?.ToList() ?? Enumerable.Empty<string>() );
        }

        // adds a range of text elements to the collection
        protected bool AddRange( TextUsageType usage, IEnumerable<string> items )
        {
            var retVal = true;

            if( usage == TextUsageType.Undefined )
            {
                _logger?.Information<TextUsageType, string>( "Cannot add {0} items ({1})",
                    usage,
                    string.Join( ",", items ) );

                return false;
            }

            foreach( var item in items ) retVal &= Add( usage, item );

            return retVal;
        }
    }
}