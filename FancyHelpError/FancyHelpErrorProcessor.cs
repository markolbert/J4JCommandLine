using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alba.CsConsoleFormat;

namespace J4JSoftware.CommandLine
{
    public class FancyHelpErrorProcessor : HelpErrorProcessor
    {
        private Document _document;
        private Grid _grid;

        public int MaxWidth { get; set; } = 80;
        public bool ShowGrid { get; set; }
        public ConsoleColor TitleColor { get; set; } = ConsoleColor.Gray;
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public ConsoleColor HelpColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor GridColor { get; set; } = ConsoleColor.Gray;

        public LineThickness GridThickness { get; set; }
            = new LineThickness(Alba.CsConsoleFormat.LineWidth.Single, Alba.CsConsoleFormat.LineWidth.Single);

        public Align HeaderAlignment { get; set; } = Align.Center;
        public Align KeyAlignment { get; set; } = Align.Center;
        public Thickness Margin { get; set; } = new Thickness(1, 0, 1, 0);

        protected override void InitializeOutput()
        {
            base.InitializeOutput();

            _document = new Document();

            _grid = new Grid { Color = GridColor };

            _document.Children.Add( _grid );
            _document.MaxWidth = MaxWidth;

            _grid.Columns.Add( GridLength.Auto, GridLength.Auto );

            if (ShowGrid)
            {
                _grid.Stroke = GridThickness;
                _grid.Color = GridColor;
            }
        }

        protected override void CreateHeaderSection()
        {
            if (!string.IsNullOrEmpty(BindingTarget.ProgramName))
            {
                var cell = new Cell(BindingTarget.ProgramName)
                {
                    Stroke = GridThickness,
                    Color = TitleColor,
                    Margin = Margin,
                    ColumnSpan = 2
                };

                _grid.Children.Add(cell);
            }

            if (!string.IsNullOrEmpty(BindingTarget.Description))
            {
                var cell = new Cell(BindingTarget.Description)
                {
                    Stroke = GridThickness,
                    Color = TitleColor,
                    Margin = Margin,
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
                    Stroke = GridThickness,
                    Color = ErrorColor,
                    Margin = Margin,
                    ColumnSpan = 2
                };

                _grid.Children.Add(cell);

                return;
            }

            // errors are displayed organized by keys
            foreach (var errorGroup in BindingTarget.Errors.OrderBy(e => e.Source.Key))
            {
                var keys = string.Join( ", ", Prefixes.ConjugateKey( errorGroup.Source.Key ) );

                _grid.Children.Add( new Cell( keys )
                {
                    Align = KeyAlignment, 
                    Color = ErrorColor, 
                    Stroke = GridThickness,
                    Margin = Margin
                });

                var errorText = string.Join( "\n", errorGroup.Errors );

                _grid.Children.Add(new Cell(errorText)
                {
                    Color = ErrorColor,
                    Stroke = GridThickness,
                    Margin = Margin
                });
            }
        }

        protected override void CreateHelpSection()
        {
            var sb = new StringBuilder();

            sb.Append( "Command line options" );

            switch( BindingTarget.KeyComparison )
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
                Color = HelpColor,
                Margin = Margin,
                ColumnSpan = 2
            } );

            // help is displayed organized by keys
            foreach( var option in BindingTarget.Options
                .OrderBy( opt => opt.FirstKey )
                .Where( opt => opt.OptionType != OptionType.Null ) )
            {
                var keys = string.Join( ", ", option.ConjugateKeys( Prefixes ) );

                _grid.Children.Add( new Cell( keys )
                {
                    Align = KeyAlignment,
                    Color = HelpColor,
                    Stroke = GridThickness,
                    Margin = Margin
                } );

                var helpText = option.Description ?? "*** no description provided ***";

                _grid.Children.Add( new Cell( helpText )
                {
                    Color = HelpColor,
                    Stroke = GridThickness,
                    Margin = Margin
                } );
            }
        }

        protected override void DisplayOutput() => ConsoleRenderer.RenderDocument( _document );
    }
}
