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

        private Token( Token toCopy )
        {
            Type = toCopy.Type;
            Text = toCopy.Text;
        }

        public Token Copy() => new Token( this );

        public TokenType Type { get; }
        public string Text { get; }
        public int Length => Text.Length;
    }

    public class TokenMatch
    {
        public TokenMatch( Token token, int startChar )
        {
            Token = token;
            StartChar = startChar;
        }

        public Token Token { get; }
        public int StartChar { get; }
    }
}
