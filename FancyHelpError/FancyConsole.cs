using System;
using System.Collections.Generic;
using System.Text;
using Alba.CsConsoleFormat;

namespace J4JSoftware.CommandLine
{
    public class FancyConsole : IConsoleOutput
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

        public void Initialize()
        {
            _document = new Document();

            _grid = new Grid { Color = GridColor };

            _document.Children.Add(_grid);
            _document.MaxWidth = MaxWidth;

            _grid.Columns.Add(GridLength.Auto, GridLength.Auto);

            if (ShowGrid)
            {
                _grid.Stroke = GridThickness;
                _grid.Color = GridColor;
            }
        }

        public void AddLine( ConsoleSection section, string? text = null )
        {
            var color = section switch
            {
                ConsoleSection.Errors => ErrorColor,
                ConsoleSection.Header => TitleColor,
                ConsoleSection.Help => HelpColor,
                _ => TitleColor
            };

            var cell = new Cell( text ?? string.Empty )
            {
                Stroke = GridThickness,
                Color = color,
                Margin = Margin,
                ColumnSpan = 2
            };

            _grid.Children.Add( cell );
        }

        public void AddError( List<string> errors, List<string>? keys = null )
        {
            _grid.Children.Add( new Cell( keys ?? new List<string>() )
            {
                Align = KeyAlignment,
                Color = ErrorColor,
                Stroke = GridThickness,
                Margin = Margin
            } );

            _grid.Children.Add( new Cell( string.Join( "\n", errors ) )
            {
                Color = ErrorColor,
                Stroke = GridThickness,
                Margin = Margin
            } );
        }

        public void AddOption( List<string> keys, string? description = null, string? defaultText = null )
        {
            _grid.Children.Add(new Cell(keys)
            {
                Align = KeyAlignment,
                Color = HelpColor,
                Stroke = GridThickness,
                Margin = Margin
            });

            var optionLines = new List<string>();
            
            optionLines.Add( description ?? "*** no description provided ***" );

            if( defaultText != null )
                optionLines.Add( defaultText );

            _grid.Children.Add( new Cell( string.Join( "\n", optionLines ) )
            {
                Color = HelpColor,
                Stroke = GridThickness,
                Margin = Margin
            } );
        }

        public void Display() => ConsoleRenderer.RenderDocument( _document );
    }
}
