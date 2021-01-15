using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IDisplayHelp
    {
        void ProcessOptions( IOptionCollection options );
    }
}
