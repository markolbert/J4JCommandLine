using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    // a class which keeps track of every text element (e.g., prefix, value encloser, quote character,
    // option key) used in the framework and ensures they are all used uniquely.
    public class MasterTextCollection
    {
        public static MasterTextCollection GetDefault( 
            CommandLineStyle cmdLineStyle, 
            CommandLineLogger logger, 
            StringComparison? comparison = null )
        {
            var retVal = new MasterTextCollection( comparison ?? StringComparison.OrdinalIgnoreCase, logger );

            switch( cmdLineStyle )
            {
                case CommandLineStyle.Linux:
                    retVal.AddRange(TextUsageType.Prefix, "-", "--");
                    retVal.AddRange(TextUsageType.Quote, "\"", "'");
                    retVal.Add(TextUsageType.ValueEncloser, "=");
                    retVal.AddRange(TextUsageType.Separator, " ", "\t");

                    return retVal;

                case CommandLineStyle.Universal:
                    retVal.AddRange(TextUsageType.Prefix, "-", "--", "/");
                    retVal.AddRange(TextUsageType.Quote, "\"", "'");
                    retVal.Add(TextUsageType.ValueEncloser, "=");
                    retVal.AddRange(TextUsageType.Separator, " ", "\t");

                    return retVal;

                case CommandLineStyle.Windows:
                    retVal.AddRange(TextUsageType.Prefix, "/", "-", "--");
                    retVal.Add(TextUsageType.Quote, "\"");
                    retVal.Add(TextUsageType.ValueEncloser, "=");
                    retVal.AddRange(TextUsageType.Separator, " ", "\t");

                    return retVal;
                
                default:
                    throw new InvalidEnumArgumentException( $"Unsupported CommandLineStyle '{cmdLineStyle}'" );
            }
        }

        private readonly List<TextUsage> _items = new List<TextUsage>();
        private readonly CommandLineLogger _logger;

        public MasterTextCollection( StringComparison comparison, CommandLineLogger logger )
        {
            TextComparison = comparison;

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

            _logger = logger;
        }

        public StringComparison TextComparison { get; }
        public IEqualityComparer<string> TextComparer { get; }

        // indicates whether the supplied text exists in the collection
        public bool Contains( string text )
            => _items.Any( x => string.Equals( x.Text, text, TextComparison ) );

        // indicates whether the supplied text exists in the collection for the specified usage
        public bool Contains( string text, TextUsageType usage ) =>
            _items.Any( x => x.Usage == usage && string.Equals( x.Text, text, TextComparison ) );

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
                _logger.LogInformation($"Duplicate {usage} '{item}'");
                return false;
            }

            if( usage == TextUsageType.Undefined )
            {
                _logger.LogInformation( $"Cannot add {usage} items ({item})" );
                return false;
            }

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

            if (usage == TextUsageType.Undefined)
            {
                _logger.LogInformation( $"Cannot add {usage} items ({string.Join( ",", items )})" );
                return false;
            }

            foreach ( var item in items )
            {
                retVal &= Add( usage, item );
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