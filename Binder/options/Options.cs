using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class Options : OptionsBase
    {
        public Options( 
            MasterTextCollection masterText, 
            IJ4JLogger logger ) 
            : base( masterText, logger )
        {
        }

        public Option Add( string contextPath ) => AddInternal( contextPath );
    }
}
