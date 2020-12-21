using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.CommandLine
{
    public abstract class OptionCollectionFactory
    {
        protected OptionCollectionFactory( 
            CommandLineStyle cmdLineStyle = CommandLineStyle.Windows,
            IAllocator? allocator = null,
            IElementTerminator? _elementTerminator = null,
            IKeyPrefixer? _keyPrefixer = null
            )
        {
            CommandLineStyle = cmdLineStyle;
            Log = new CommandLineLogger();
            MasterText = MasterTextCollection.GetDefault( cmdLineStyle );
            ElementTerminator = _elementTerminator ?? new ElementTerminator( MasterText, Log );
            KeyPrefixer = _keyPrefixer ?? new KeyPrefixer( MasterText, Log );
            Allocator = allocator ?? new Allocator( ElementTerminator, KeyPrefixer, Log );
        }

        protected OptionCollectionFactory( 
            MasterTextCollection mt,
            IAllocator? allocator = null,
            IElementTerminator? _elementTerminator = null,
            IKeyPrefixer? _keyPrefixer = null
            )
        {
            CommandLineStyle = CommandLineStyle.UserDefined;
            Log = new CommandLineLogger();
            MasterText = mt;
            ElementTerminator = _elementTerminator ?? new ElementTerminator(MasterText, Log);
            KeyPrefixer = _keyPrefixer ?? new KeyPrefixer(MasterText, Log);
            Allocator = allocator ?? new Allocator(ElementTerminator, KeyPrefixer, Log);
        }

        public CommandLineStyle CommandLineStyle { get; }
        public CommandLineLogger Log { get; }
        public MasterTextCollection MasterText { get; }
        public IElementTerminator ElementTerminator { get; }
        public IKeyPrefixer KeyPrefixer { get; }
        public IAllocator Allocator { get; }

        public OptionCollection GetOptionCollection()
        {
            var retVal = new OptionCollection( MasterText, Log );

            InitializeOptions( retVal );

            return retVal;
        }

        protected abstract void InitializeOptions( OptionCollection options );
    }
}
