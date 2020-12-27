namespace J4JSoftware.CommandLine
{
    public partial class AllocationElement
    {
        private readonly MasterTextCollection _mt;
        
        private AllocationElement( 
            AllocationContext2 context, 
            int startChar, 
            int endChar,
            MasterTextCollection mt
            )
        {
            Context = context;
            
            StartChar = startChar;
            EndChar = endChar;

            _mt = mt;
        }
        
        public AllocationContext2 Context { get; }
        
        public int StartChar { get; private set; }
        public int EndChar { get; private set; }
        public string Text => Context.Text[ StartChar..EndChar ];
        public TextUsageType TextType =>_mt.GetTextUsageType( Text );

        public bool NextChar()
        {
            var newEnd = EndChar + 1;

            if( newEnd <= Context.Text.Length - 1 )
            {
                EndChar = newEnd;
                return true;
            }

            return false;
        }

        public bool PreviousChar()
        {
            var newEnd = EndChar - 1;

            if( newEnd >= StartChar )
            {
                EndChar = newEnd;
                return true;
            }

            return false;
        }
    }
}