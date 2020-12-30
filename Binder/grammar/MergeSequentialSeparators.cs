using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class MergeSequentialSeparators : ICleanupTokens
    {
        private readonly IJ4JLogger? _logger;

        public MergeSequentialSeparators( IJ4JLogger? logger )
        {
            _logger = logger;
        }

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