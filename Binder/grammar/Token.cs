using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.CommandLine
{
    public class Token
    {
        public Token( 
            TokenType type,
            string text
            )
        {
            Type = type;
            Text = text;
        }

        public TokenType Type { get; }
        public string Text { get; }
        public int Length => Text.Length;
    }
}
