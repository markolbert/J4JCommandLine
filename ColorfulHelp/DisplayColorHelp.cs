using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Alba.CsConsoleFormat;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.J4JCommandLine
{
    [ Flags ]
    public enum Borders
    {
        Left = 1 << 0,
        Top = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,

        None = 0,
        NoBottom = Left | Top | Right,
        NoTop = Left | Bottom | Right,
        NoLeft = Top | Bottom | Right,

        All = Left | Top | Right | Bottom
    }

    public class DisplayColorHelp : DisplayHelpBase
    {
        public DisplayColorHelp( IJ4JLogger? logger ) 
            : base( logger )
        {
        }

        public Thickness CellPadding { get; set; } = new Thickness( 2, 0 );
        public ConsoleColor HeadingColor { get; set; } = ConsoleColor.Green;
        public ConsoleColor EmphasisColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor KeysColor { get; set; } = ConsoleColor.Cyan;
        public ConsoleColor TextColor { get; set; } = ConsoleColor.White;

        public override void ProcessOptions( IOptionCollection options )
        {
            var grid = new Grid { Color = ConsoleColor.Gray };

            grid.Columns.Add( GridLength.Auto, GridLength.Auto, GridLength.Auto );

            grid.Children.Add( NewCell( "Key(s)", HeadingColor ),
                NewCell( "Required", HeadingColor ),
                NewCell( "Description", HeadingColor ) );

            grid.Children.Add( options.Select( x =>
            {
                var retVal = new List<object>();

                retVal.Add( NewCell( string.Join( ", ", GetKeys( x ) ), KeysColor, border: Borders.NoBottom  ) );
                retVal.Add( NewCell( x.Required ? "Y" : "N", KeysColor ) );
                retVal.Add( NewCell( x.Description, TextColor ) );

                var styleDefCell = NewCell( colSpan: 2,
                    align: Align.Left,
                    padding: new Thickness( 2, 0, 2, 0 ));

                styleDefCell.Children.Add( new Span( GetStyleText( x ) ) { Color = TextColor } );

                var defValue = x.GetDefaultValue();

                if( !string.IsNullOrEmpty( defValue ) )
                    styleDefCell.Children.Add( "\n",
                        new Span( "default: " ) { Color = HeadingColor },
                        new Span( defValue ) { Color = KeysColor } );

                retVal.Add( NewCell( border: Borders.Bottom | Borders.Left) );
                retVal.Add( styleDefCell );

                return retVal.ToArray();
            } ) );

            var doc = new Document();

            doc.Children.Add( new Span( "Command line help" ) { Color = EmphasisColor }, "\n" );
            doc.Children.Add( grid );

            ConsoleRenderer.RenderDocument( doc );
        }

        private Cell NewCell( 
            string? content = null, 
            ConsoleColor? color = null, 
            Align? align = null,
            int colSpan = 1, 
            Thickness? padding = null,
            Borders border = Borders.All )
        {
            color ??= ConsoleColor.White;
            align ??= Align.Center;
            padding ??= CellPadding;

            return new Cell( content )
            {
                Align = align.Value,
                Color = color,
                Padding = padding.Value,
                ColumnSpan = colSpan,
                Stroke = GetStroke(border)
            };
        }

        private LineThickness NoRightBorder() =>
            new LineThickness( LineWidth.Single, LineWidth.Single, LineWidth.None, LineWidth.Single );

        private LineThickness NoLeftBorder() =>
            new LineThickness( LineWidth.None, LineWidth.Single, LineWidth.Single, LineWidth.Single );

        private LineThickness NoBottomBorder()=>
            new LineThickness( LineWidth.Single, LineWidth.Single, LineWidth.Single, LineWidth.None);

        private LineThickness NoTopBorder()=>
            new LineThickness( LineWidth.Single, LineWidth.None, LineWidth.Single, LineWidth.Single);

        private LineThickness GetStroke( Borders sides )
        {
            var retVal = new LineThickness();

            retVal.Bottom = (sides & Borders.Bottom) == Borders.Bottom ? LineWidth.Single : LineWidth.None;
            retVal.Left = (sides & Borders.Left) == Borders.Left ? LineWidth.Single : LineWidth.None;
            retVal.Right = (sides & Borders.Right) == Borders.Right ? LineWidth.Single : LineWidth.None;
            retVal.Top = (sides & Borders.Top) == Borders.Top ? LineWidth.Single : LineWidth.None;

            return retVal;
        }
    }
}
