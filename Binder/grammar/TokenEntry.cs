using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class TokenEntry
    {
        private TokenEntry( TokenEntries entries )
        {
            Entries = entries;
        }

        public TokenEntries Entries { get; }
        public string? Key { get; set; }
        public List<string> Values { get; } = new();

        public IOption? Option => Entries.Options.FirstOrDefault( x =>
            x.Keys.Any( k => k.Equals( Key, Entries.Options.MasterText.TextComparison ) ) );
    }
}