using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;

namespace J4JSoftware.CommandLine
{
    // a class which keeps track of every text element (e.g., prefix, value encloser, quote character,
    // option key) used in the framework and ensures they are all used uniquely.
    public class MasterTextCollection
    {
        private readonly List<TextUsage> _items = new List<TextUsage>();

        public bool IsValid => TextComparer != null && TextComparison != null;

        public StringComparison? TextComparison { get; private set; }
        public IEqualityComparer<string>? TextComparer { get; private set; }

        public void Initialize( StringComparison textComp )
        {
            TextComparison = textComp;

            TextComparer = textComp switch
            {
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.Ordinal => StringComparer.OrdinalIgnoreCase,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                _ => StringComparer.CurrentCultureIgnoreCase
            };
        }

        // indicates whether the supplied text exists in the collection
        public bool Contains( string text )
            => IsValid && _items.Any( x => string.Equals( x.Text, text, TextComparison!.Value ) );

        // indicates whether the supplied text exists in the collection for the specified usage
        public bool Contains( string text, TextUsageType usage ) =>
            IsValid && _items.Any( x => x.Usage == usage && string.Equals( x.Text, text, TextComparison!.Value ) );

        // adds an item to the collection
        public bool Add( TextUsageType usage, string item )
        {
            if( Contains( item ) )
                return false;

            _items.Add( new TextUsage( item, usage ) );

            return true;
        }

        // adds a range of text elements to the collection
        public bool AddRange( TextUsageType usage, params string[] items )
            => AddRange( usage, items?.ToList() ?? Enumerable.Empty<string>() );

        // adds a range of text elements to the collection
        public bool AddRange( TextUsageType usage, IEnumerable<string> items )
        {
            var retVal = true;

            foreach( var item in items )
            {
                if( Contains( item ) )
                    retVal = false;
                else _items.Add( new TextUsage( item, usage ) );
            }

            return retVal;
        }

        // gets a list of all the items for the specified type of usage
        public List<string> this[ TextUsageType usage ] =>
            _items.Where( x => x.Usage == usage )
                .Select( x => x.Text )
                .ToList();
    }
}