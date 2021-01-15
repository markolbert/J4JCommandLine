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

            grid.Children.Add( CenteredCell( "Key(s)", HeadingColor ),
                CenteredCell( "Required", HeadingColor ),
                CenteredCell( "Description", HeadingColor ) );

            grid.Children.Add( options.Select( x =>
            {
                var retVal = new List<object>();

                retVal.Add( CenteredCell( string.Join( ", ", GetKeys( x ) ), KeysColor ) );
                retVal.Add( CenteredCell( x.Required ? "Y" : "N", KeysColor ) );
                retVal.Add( CenteredCell( x.Description, TextColor ) );
                retVal.Add( new Cell() );
                retVal.Add( new Cell( GetStyleText( x ) )
                    { Color = TextColor, Padding = CellPadding, ColumnSpan = 2 } );

                var defValue = x.GetDefaultValue();

                if( string.IsNullOrEmpty( defValue ) ) 
                    return retVal.ToArray();

                retVal.Add(new Cell());

                var defCell = new Cell() { ColumnSpan = 2, Padding = CellPadding };
                defCell.Children.Add("default: ".White(), new Span(defValue){Color = KeysColor}  );

                retVal.Add( defCell );

                return retVal.ToArray();
            } ) );

            var doc = new Document();

            doc.Children.Add( new Span( "Command line help" ) { Color = EmphasisColor }, "\n" );
            doc.Children.Add( grid );

            ConsoleRenderer.RenderDocument( doc );
        }

        private Cell CenteredCell( string content, ConsoleColor color, int colSpan = 1 ) =>
            new Cell( content )
            {
                Align = Align.Center,
                Color = color,
                Padding = CellPadding,
                ColumnSpan = colSpan,
                Stroke = LineThickness.Single
            };
    }
}
