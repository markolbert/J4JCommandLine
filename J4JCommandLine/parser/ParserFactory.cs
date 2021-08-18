#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class ParserFactory : IParserFactory
    {
        private readonly List<IAvailableTokens> _tokens;
        private readonly List<IMasterTextCollection> _mtCollections;
        private readonly List<IBindabilityValidator> _bindabilityValidators;
        private readonly List<IOptionsGenerator> _generators;
        private readonly IDisplayHelp _displayHelp;

        private readonly IJ4JLoggerFactory? _loggerFactory;
        private readonly IJ4JLogger? _logger;

        public ParserFactory(
            IEnumerable<IAvailableTokens> tokens,
            IEnumerable<IMasterTextCollection> mtCollections,
            IEnumerable<IBindabilityValidator> bindabilityValidators,
            IEnumerable<IOptionsGenerator> generators,
            IDisplayHelp displayHelp,
            IJ4JLoggerFactory? loggerFactory = null
        )
        {
            _tokens = tokens.OrderByDescending(x=>x.Customization)
                .ThenByDescending(x=>x.Priority)
                .ToList();

            _mtCollections = mtCollections.OrderByDescending(x => x.Customization)
                .ThenByDescending(x => x.Priority)
                .ToList();

            _bindabilityValidators = bindabilityValidators.OrderByDescending(x => x.Customization)
                .ThenByDescending(x => x.Priority)
                .ToList();

            _generators = generators.OrderByDescending( x => x.Customization )
                .ThenByDescending( x => x.Priority )
                .ToList();

            _displayHelp = displayHelp;

            _loggerFactory = loggerFactory;
            _logger = _loggerFactory?.CreateLogger( GetType() );
        }

        public bool Create( CommandLineStyle style,
            out IParser? result,
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens )
        {
            textComparison ??= style.GetStringComparison();

            result = null;

            var masterText = _mtCollections.FirstOrDefault( x => x.Style == style );
            if( masterText == null )
            {
                _logger?.Error( "No IMasterTextCollection available for CommandLineStyle '{0}'", style );
                return false;
            }

            masterText.Initialize();

            var bindabilityValidator = _bindabilityValidators.FirstOrDefault( x => x.Style == style );
            if( bindabilityValidator == null )
            {
                _logger?.Error( "No IBindabilityValidator available for CommandLineStyle '{0}'", style );
                return false;
            }

            var tokens = _tokens.FirstOrDefault( x => x.Style == style );
            if( tokens == null )
            {
                _logger?.Error( "No IAvailableTokens available for CommandLineStyle '{0}'", style );
                return false;
            }

            tokens.Initialize();

            _displayHelp.Initialize( masterText );

            var optionCollection = new OptionCollection( textComparison.Value,
                masterText,
                bindabilityValidator,
                _displayHelp,
                _loggerFactory?.CreateLogger<OptionCollection>() );

            var generator = _generators.FirstOrDefault();
            if (generator == null)
            {
                _logger?.Error("No IOptionsGenerator available");
                return false;
            }

            generator.Initialize( textComparison.Value, optionCollection );

            var tokenizer = new Tokenizer( textComparison.Value, tokens, _loggerFactory, cleanupTokens );

            result = new Parser( optionCollection,
                new ParsingTable( generator, _loggerFactory?.CreateLogger<IOptionsGenerator>() ),
                tokenizer,
                _loggerFactory?.CreateLogger<Parser>() );

            return true;
        }
    }
}