using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public partial class AllocationElement
    {
        public class AllocationContext2
        {
            private readonly Stack<AllocationElement> _elements = new Stack<AllocationElement>();
            private readonly MasterTextCollection _mt;
            
            public AllocationContext2( string text, MasterTextCollection mt )
            {
                Text = text;
                _mt = mt;

                _elements.Push( new AllocationElement( this, 0, 0, _mt ) );
            }

            public string Text { get; }
            public AllocationElement CurrentElement => _elements.Peek();
            public bool HasQuoteElement => _elements.Any( x => x.TextType == TextUsageType.Quote );

            public AllocationElement? QuotingElement =>
                _elements.FirstOrDefault( x => x.TextType == TextUsageType.Quote );

            public AllocationElement CreateNextElement()
            {
                var retVal = new AllocationElement( this, CurrentElement.EndChar + 1, CurrentElement.EndChar + 1, _mt );
                _elements.Push( retVal );

                return retVal;
            }
        }
    }
}