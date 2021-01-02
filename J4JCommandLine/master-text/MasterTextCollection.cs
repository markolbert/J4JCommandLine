﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine.Deprecated
{
    // a class which keeps track of every text element (e.g., prefix, value encloser, quote character,
    // option key) used in the framework and ensures they are all used uniquely.
    public class MasterTextCollection
    {
        private readonly List<TextUsage> _items = new List<TextUsage>();

        // creates an instance which uses the StringComparison parameter to determine
        // case sensitivity
        public MasterTextCollection( StringComparison textComp )
        {
            TextComparison = textComp;
        }

        public StringComparison TextComparison { get; }

        // indicates whether the supplied text exists in the collection
        public bool Contains( string text ) =>
            _items.Any( x => string.Equals( x.Text, text, TextComparison ) );

        // indicates whether the supplied text exists in the collection for the specified usage
        public bool Contains( string text, TextUsageType usage ) =>
            _items.Any( x => x.Usage == usage && string.Equals( x.Text, text, TextComparison ) );

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