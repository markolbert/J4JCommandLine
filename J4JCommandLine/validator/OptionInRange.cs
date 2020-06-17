using System;
using System.Text;

namespace J4JSoftware.CommandLine
{
    // determines whether a Type which supports the IComparable<T> interface 
    // meets certain inclusion criteria (e.g., >, >=). Only Types supporting IComparable<T>
    // can be validated by this class.
    //
    // Instances of this class should not be created directly. Instead, call one of the static
    // creation methods defined in OptionInRangeStatic.cs
    public partial class OptionInRange<T> : OptionValidator<T>
        where T : IComparable<T>
    {
        protected OptionInRange()
        {
        }

        public T Minimum { get; private set; }
        public bool IncludeMinimumEqual { get; private set; }
        protected bool MinimumSet { get; set; }

        public T Maximum { get; private set; }
        public bool IncludeMaximumEqual { get; private set; }
        protected bool MaximumSet { get; set; }

        public override bool Validate( Option option, T value, CommandLineLogger logger )
        {
            if( IsValid( value ) )
                return true;

            var sb = new StringBuilder();

            sb.Append( $"'{value}' must be " );

            if( MinimumSet )
            {
                sb.Append( IncludeMinimumEqual ? ">=" : ">" );
                sb.Append( $" {Minimum}" );
            }

            if( MaximumSet )
            {
                if( MinimumSet ) sb.Append( " and " );

                sb.Append( IncludeMaximumEqual ? "<=" : "<" );
                sb.Append( $" {Maximum}" );
            }

            logger.LogError( ProcessingPhase.Validating, sb.ToString(), option : option );

            return false;
        }

        private bool IsValid( T toCheck )
        {
            if( MinimumSet )
            {
                var comparison = Minimum.CompareTo( toCheck );

                if( comparison > 0 ) return false;
                if( !IncludeMinimumEqual && comparison == 0 ) return false;
            }

            if( MaximumSet )
            {
                var comparison = Maximum.CompareTo( toCheck );

                if( comparison < 0 ) return false;
                if( !IncludeMaximumEqual && comparison == 0 ) return false;
            }

            return true;
        }
    }
}