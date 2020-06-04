using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alba.CsConsoleFormat;

namespace J4JSoftware.CommandLine
{
    public class FancyHelpErrorProcessor : HelpErrorProcessor
    {
        private readonly FancyOutputConfiguration _outConfig;

        private Document _document;
        private Grid _grid;

        public FancyHelpErrorProcessor( 
            IParsingConfiguration parseConfig, 
            FancyOutputConfiguration outputConfig 
            ) : base( parseConfig )
        {
            _outConfig = outputConfig;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _document = new Document();

            _grid = new Grid { Color = _outConfig.GridColor };

            _document.Children.Add( _grid );
            _document.MaxWidth = _outConfig.MaxWidth;

            _grid.Columns.Add( GridLength.Auto, GridLength.Auto );

            if (_outConfig.ShowGrid)
            {
                _grid.Stroke = _outConfig.GridThickness;
                _grid.Color = _outConfig.GridColor;
            }
        }

        protected override void CreateHeaderSection()
        {
            if (!string.IsNullOrEmpty(ParsingConfiguration.ProgramName))
            {
                var cell = new Cell(ParsingConfiguration.ProgramName)
                {
                    Stroke = _outConfig.GridThickness,
                    Color = _outConfig.TitleColor,
                    Margin = _outConfig.Margin,
                    ColumnSpan = 2
                };

                _grid.Children.Add(cell);
            }

            if (!string.IsNullOrEmpty(ParsingConfiguration.Description))
            {
                var cell = new Cell(ParsingConfiguration.Description)
                {
                    Stroke = _outConfig.GridThickness,
                    Color = _outConfig.TitleColor,
                    Margin = _outConfig.Margin,
                    ColumnSpan = 2
                };

                _grid.Children.Add(cell);
            }
        }

        protected override void CreateErrorSection()
        {
            if (BindingTarget.Errors.Count == 0)
            {
                var cell = new Cell("Errors were encountered but not described")
                {
                    Stroke = _outConfig.GridThickness,
                    Color = _outConfig.ErrorColor,
                    Margin = _outConfig.Margin,
                    ColumnSpan = 2
                };

                _grid.Children.Add(cell);

                return;
            }

            // errors are displayed organized by keys
            foreach (var errorGroup in BindingTarget.Errors.OrderBy(e => e.Source.Key))
            {
                var keys = ParsingConfiguration.ConjugateKey( errorGroup.Source.Key );

                _grid.Children.Add( new Cell( keys )
                {
                    Align = _outConfig.KeyAlignment, 
                    Color = _outConfig.ErrorColor, 
                    Stroke = _outConfig.GridThickness,
                    Margin = _outConfig.Margin
                });

                var errorText = string.Join( "\n", errorGroup.Errors );

                _grid.Children.Add(new Cell(errorText)
                {
                    Color = _outConfig.ErrorColor,
                    Stroke = _outConfig.GridThickness,
                    Margin = _outConfig.Margin
                });
            }
        }

        protected override void CreateHelpSection()
        {
            var sb = new StringBuilder();

            sb.Append( "Command line options" );

            switch( ParsingConfiguration.TextComparison )
            {
                case StringComparison.Ordinal:
                case StringComparison.InvariantCulture:
                case StringComparison.CurrentCulture:
                    sb.Append( " (case sensitive):" );
                    break;

                default:
                    sb.Append( ":" );
                    break;
            }

            _grid.Children.Add( new Cell(sb.ToString())
            {
                Color = _outConfig.HelpColor,
                Margin = _outConfig.Margin,
                ColumnSpan = 2
            } );

            // help is displayed organized by keys
            foreach( var option in BindingTarget.Options
                .OrderBy( opt => opt.FirstKey )
                .Where( opt => opt.OptionType != OptionType.Null ) )
            {
                var keys = string.Join( ", ", option.ConjugateKeys( ParsingConfiguration ) );

                _grid.Children.Add( new Cell( keys )
                {
                    Align = _outConfig.KeyAlignment,
                    Color = _outConfig.HelpColor,
                    Stroke = _outConfig.GridThickness,
                    Margin = _outConfig.Margin
                } );

                var helpText = option.Description ?? "*** no description provided ***";

                _grid.Children.Add( new Cell( helpText )
                {
                    Color = _outConfig.HelpColor,
                    Stroke = _outConfig.GridThickness,
                    Margin = _outConfig.Margin
                } );
            }
        }

        protected override void DisplayOutput() => ConsoleRenderer.RenderDocument( _document );
    }
}
