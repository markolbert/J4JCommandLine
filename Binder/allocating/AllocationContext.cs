using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class AllocationContext
    {
        private readonly MasterTextCollection _mt;
        
        private int _startChar;
        private int _endChar;
        
        public AllocationContext( MasterTextCollection mt, string text )
        {
            _mt = mt;
            Text = text;
        }
        
        public string Text { get;  }
        public int Length => Text.Length;
        public bool PastEndOfText => EndChar >= Text.Length;
        
        public int StartChar
        {
            get => _startChar;
            
            set
            {
                if( value <= 0 || value > _endChar )
                    return;

                _startChar = value;
            }
        }

        public int EndChar
        {
            get => _endChar;

            set
            {
                if (value <= 0 || value < _startChar )
                    return;

                _endChar = value;
            }
        }

        public TextUsageType ElementUsage => _mt.GetTextUsageType( Element );

        public bool StartsWith( TextUsageType usage, out string? prefix )
        {
            prefix = _mt[ usage ].FirstOrDefault( x => Element.IndexOf( x, _mt.TextComparison ) == 0 );

            return prefix != null;
        }

        public bool EndsWith( TextUsageType usage, out string? suffix )
        {
            suffix = _mt[ usage ].FirstOrDefault( x =>
                Element.LastIndexOf( x, _mt.TextComparison ) == Element.Length - x.Length );

            return suffix != null;
        }

        public string Element => Text[ StartChar..EndChar ];

        public string GetTrimmedElement( int toTrim )
        {
            var adjEnd = EndChar - toTrim;

            if( adjEnd < StartChar )
                return string.Empty;

            return Text[ StartChar..adjEnd ];
        }
    }
}