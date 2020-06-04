namespace J4JSoftware.CommandLine
{
    public class OutputConfiguration //: IOutputConfiguration
    {
        private int _keyWidth = 20;
        private int _keyDetailSep = 5;
        private int _detailWidth = 55;

        public int LineWidth => KeyAreaWidth + KeyDetailSeparation + DetailAreaWidth;

        public int KeyAreaWidth
        {
            get => _keyWidth;

            set
            {
                if( value > 0 )
                    _keyWidth = value;
            }
        }

        public int DetailAreaWidth
        {
            get => _detailWidth;

            set
            {
                if( value > 0 )
                    _detailWidth = value;
            }
        }

        public int KeyDetailSeparation
        {
            get => _keyDetailSep;

            set
            {
                if (value > 0) 
                    _keyDetailSep = value;
            }
        }
    }
}