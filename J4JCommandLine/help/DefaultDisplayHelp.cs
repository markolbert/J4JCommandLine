using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class DefaultDisplayHelp : DisplayHelpBase
    {
        public DefaultDisplayHelp( IJ4JLogger? logger )
            : base( logger )
        {
        }

        public override void ProcessOptions( IOptionCollection options )
        {
            Console.WriteLine( "Command line help\n" );

            foreach( var option in options )
            {
                Console.WriteLine( $"Keys: {string.Join( ", ", GetKeys( option ) )}" );
                Console.WriteLine($"Description:  {option.Description}");
                Console.WriteLine( $"Style:  {GetStyleText( option )}" );

                var defaultValue = option.GetDefaultValue();
                if( !string.IsNullOrEmpty(defaultValue))
                    Console.WriteLine($"Default Value:  {defaultValue}");

                if( option.Required )
                    Console.WriteLine( "Required" );

                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
