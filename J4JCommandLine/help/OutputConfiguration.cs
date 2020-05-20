using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class OutputConfiguration : IOutputConfiguration
    {
        private int _keyWidth = 20;
        private int _keyDetailSep = 5;
        private int _detailWidth = 55;

        public OutputConfiguration( IJ4JLogger? logger )
        {
            Logger = logger;

            Logger?.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public int LineWidth => KeyAreaWidth + KeyDetailSeparation + DetailAreaWidth;

        public int KeyAreaWidth
        {
            get => _keyWidth;

            set
            {
                if( value <= 0 )
                {
                    Logger?.Error( "Attempting to set KeyAreaWidth to <= 0, ignoring" );
                    return;
                }

                _keyWidth = value;
            }
        }

        public int DetailAreaWidth
        {
            get => _detailWidth;

            set
            {
                if (value <= 0)
                {
                    Logger?.Error("Attempting to set DetailAreaWidth to <= 0, ignoring");
                    return;
                }

                _detailWidth = value;
            }
        }

        public int KeyDetailSeparation
        {
            get => _keyDetailSep;

            set
            {
                if (value <= 0)
                {
                    Logger?.Error("Attempting to set KeyDetailSeparation to <= 0, ignoring");
                    return;
                }

                _keyDetailSep = value;
            }
        }
    }
}