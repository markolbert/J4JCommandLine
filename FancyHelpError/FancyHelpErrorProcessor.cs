using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alba.CsConsoleFormat;

namespace J4JSoftware.CommandLine
{
    public interface IFancyOutputConfiguration : IOutputConfiguration
    {
        bool FrameErrors { get; set; }
        bool FrameHelp { get; set; }
        bool ShowGrid { get; set; }
        ConsoleColor TitleColor { get; set; }
        ConsoleColor ErrorColor { get; set; }
        ConsoleColor HelpColor { get; set; }
        ConsoleColor GridColor { get; set; }
        LineThickness GridThickness { get; set; }
        Align HeaderAlignment { get; set; }
        Align KeyAlignment { get; set; }
    }

    public class FancyOutputConfiguration : OutputConfiguration, IFancyOutputConfiguration
    {
        public bool FrameErrors { get; set; }
        public bool FrameHelp { get; set; }
        public bool ShowGrid { get; set; }
        public ConsoleColor TitleColor { get; set; } = ConsoleColor.Gray;
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public ConsoleColor HelpColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor GridColor { get; set; } = ConsoleColor.Gray;

        public LineThickness GridThickness { get; set; }
            = new LineThickness( Alba.CsConsoleFormat.LineWidth.Single, Alba.CsConsoleFormat.LineWidth.Single );

        public Align HeaderAlignment { get; set; } = Align.Center;
        public Align KeyAlignment { get; set; } = Align.Center;
    }

    public class FancyHelpErrorProcessor : HelpErrorProcessor
    {
        public FancyHelpErrorProcessor( 
            IParsingConfiguration parseConfig, 
            IFancyOutputConfiguration outputConfig 
            ) : base( parseConfig, outputConfig )
        {
        }

        private IFancyOutputConfiguration FancyConfig => (IFancyOutputConfiguration) OutputConfiguration;

        protected override void DisplayHeader()
        {
            var document = new Document();

            var grid = new Grid
            {
                Columns = { GridLength.Auto },
                Align = FancyConfig.HeaderAlignment
            };

            document.Children.Add(grid);

            if (FancyConfig.ShowGrid)
            {
                grid.Stroke = FancyConfig.GridThickness;
                grid.Color = FancyConfig.GridColor;
            }

            if (!string.IsNullOrEmpty(ParsingConfiguration.ProgramName))
            {
                var cell = new Cell(ParsingConfiguration.ProgramName)
                {
                    Stroke = FancyConfig.GridThickness,
                    Color = FancyConfig.TitleColor
                };

                grid.Children.Add(cell);
            }

            if (!string.IsNullOrEmpty(ParsingConfiguration.Description))
            {
                var cell = new Cell(ParsingConfiguration.Description)
                {
                    Stroke = FancyConfig.GridThickness,
                    Color = FancyConfig.TitleColor
                };

                grid.Children.Add(cell);
            }

            ConsoleRenderer.RenderDocument(document);
        }

        protected override void DisplayErrors()
        {
            var document = new Document();

            var grid = new Grid { Color = FancyConfig.GridColor };

            document.Children.Add( grid );

            if (BindingTarget.Errors.Count == 0)
            {
                grid.Columns.Add( GridLength.Auto );
                grid.Align = FancyConfig.HeaderAlignment;

                var cell = new Cell("Errors were encountered but not described")
                {
                    Stroke = FancyConfig.GridThickness,
                    Color = FancyConfig.ErrorColor
                };

                grid.Children.Add(cell);

                ConsoleRenderer.RenderDocument(document);

                return;
            }

            grid.Columns.Add( GridLength.Auto, GridLength.Auto );

            // errors are displayed organized by keys
            foreach (var errorGroup in BindingTarget.Errors.OrderBy(e => e.Source.Key))
            {
                var keys = ParsingConfiguration.ConjugateKey( errorGroup.Source.Key );

                grid.Children.Add( new Cell( keys )
                {
                    Align = FancyConfig.KeyAlignment, 
                    Color = FancyConfig.ErrorColor, 
                    Stroke = FancyConfig.GridThickness
                } );

                var errorText = string.Join( "\n", errorGroup.Errors );

                grid.Children.Add(new Cell(errorText)
                {
                    Color = FancyConfig.ErrorColor,
                    Stroke = FancyConfig.GridThickness
                });
            }

            ConsoleRenderer.RenderDocument( document );
        }

        protected override void DisplayHelp()
        {
            var document = new Document();

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

            document.Children.Add(
                new Span( sb.ToString() )
                {
                    Color = FancyConfig.HelpColor
                } );

            var grid = new Grid { Color = FancyConfig.GridColor };

            document.Children.Add( grid );

            grid.Columns.Add( GridLength.Auto, GridLength.Auto );

            // errors are displayed organized by keys
            foreach( var option in BindingTarget.Options
                .OrderBy( opt => opt.FirstKey )
                .Where( opt => opt.OptionType != OptionType.Null ) )
            {
                var keys = option.ConjugateKeys( ParsingConfiguration );

                grid.Children.Add( new Cell( keys )
                {
                    Align = FancyConfig.KeyAlignment,
                    Color = FancyConfig.HelpColor,
                    Stroke = FancyConfig.GridThickness
                } );

                var helpText = option.Description ?? "*** no description provided ***";

                grid.Children.Add( new Cell( helpText )
                {
                    Color = FancyConfig.HelpColor,
                    Stroke = FancyConfig.GridThickness
                } );
            }

            ConsoleRenderer.RenderDocument( document );
        }
    }
}
