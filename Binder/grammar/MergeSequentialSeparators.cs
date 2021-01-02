using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine
{
    public class MergeSequentialSeparators : ICleanupTokens
    {
        public void Process( List<Token> tokens )
        {
            var toRemove = new List<int>();

            Token? prevToken = null;

            for( var idx = 0; idx < tokens.Count; idx++ )
            {
                var token = tokens[ idx ];

                if( token.Type == TokenType.Separator && prevToken?.Type == token.Type )
                    toRemove.Add( idx );

                prevToken = token;
            }

            tokens.RemoveRange( toRemove );
        }
    }
}