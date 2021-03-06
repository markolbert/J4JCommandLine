﻿using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    // a class which keeps track of every text element (e.g., prefix, value encloser, quote character,
    // option key) used in the framework and ensures they are all used uniquely.
    public partial class MasterTextCollection
    {
        private readonly List<TextUsage> _items = new();
        private readonly IJ4JLogger? _logger;

        public MasterTextCollection( StringComparison comparison, Func<IJ4JLogger>? loggerFactory = null )
        {
            TextComparison = comparison;
            LoggerFactory = loggerFactory;

            TextComparer = comparison switch
            {
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.Ordinal => StringComparer.OrdinalIgnoreCase,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                _ => StringComparer.CurrentCultureIgnoreCase
            };

            _logger = loggerFactory?.Invoke();
        }

        public StringComparison TextComparison { get; }
        public IEqualityComparer<string> TextComparer { get; }

        internal Func<IJ4JLogger>? LoggerFactory { get; }

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
        public bool AddRange( TextUsageType usage, params string[] items )
        {
            return AddRange( usage, items?.ToList() ?? Enumerable.Empty<string>() );
        }

        // adds a range of text elements to the collection
        public bool AddRange( TextUsageType usage, IEnumerable<string> items )
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