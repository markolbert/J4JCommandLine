using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.CommandLine
{
    public class NewAllocator
    {
        private readonly MasterTextCollection _mt;
        private readonly CommandLineLogger _logger;

        public NewAllocator(
            MasterTextCollection mt,
            CommandLineLogger logger
            )
        {
            _mt = mt;
            _logger = logger;
        }

        public void AllocateCommandLine( string cmdLine, OptionCollection options )
        {
            options.ClearValues();
            
            var context = new AllocationElement.AllocationContext2( cmdLine, _mt );

            while( context.CurrentElement.EndChar < context.Text.Length )
            {
                var curUsage = context.CurrentElement.TextType;

                while( context.CurrentElement.TextType == curUsage )
                {
                    // if we can't move forward we're at the end of the cmd line
                    if( !context.CurrentElement.NextChar() )
                    {
                        EmitResults();
                        return;
                    }

                    curUsage = context.CurrentElement.TextType;
                }

                // back off one character, to the end of the previous text type sequence
                // if this fails the cmd line must've been empty so we're done
                if (!context.CurrentElement.PreviousChar())
                    return;
                
                // create and move to a new element
                context.CreateNextElement();
            }
        }

        private void EmitResults()
        {
        }
    }
}
