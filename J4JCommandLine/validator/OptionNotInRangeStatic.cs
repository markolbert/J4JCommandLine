using System;

namespace J4JSoftware.CommandLine.Deprecated
{
    public partial class OptionNotInRange<T> : OptionValidator<T>
        where T : IComparable<T>
    {
        public static OptionNotInRange<TProp> GreaterThan<TProp>( TProp min ) where TProp : IComparable<TProp>
        {
            return new OptionNotInRange<TProp> { Minimum = min, MinimumSet = true };
        }

        public static OptionNotInRange<TProp> GreaterThanEqual<TProp>( TProp min ) where TProp : IComparable<TProp>
        {
            return new OptionNotInRange<TProp> { Minimum = min, IncludeMinimumEqual = true, MinimumSet = true };
        }

        public static OptionNotInRange<TProp> LessThan<TProp>( TProp max ) where TProp : IComparable<TProp>
        {
            return new OptionNotInRange<TProp> { Maximum = max, MaximumSet = true };
        }

        public static OptionNotInRange<TProp> LessThanEqual<TProp>( TProp max ) where TProp : IComparable<TProp>
        {
            return new OptionNotInRange<TProp> { Maximum = max, IncludeMaximumEqual = true, MaximumSet = true };
        }

        public static OptionNotInRange<TProp> GreaterLessThan<TProp>( TProp min, TProp max )
            where TProp : IComparable<TProp>
        {
            return new OptionNotInRange<TProp> { Minimum = min, Maximum = max, MinimumSet = true, MaximumSet = true };
        }

        public static OptionNotInRange<TProp> GreaterLessThanEqual<TProp>( TProp min, TProp max )
            where TProp : IComparable<TProp>
        {
            return new OptionNotInRange<TProp>
            {
                Minimum = min, IncludeMinimumEqual = true, Maximum = max, IncludeMaximumEqual = true, MinimumSet = true,
                MaximumSet = true
            };
        }

        public static OptionNotInRange<TProp> GreaterEqualLessThan<TProp>( TProp min, TProp max )
            where TProp : IComparable<TProp>
        {
            return new OptionNotInRange<TProp>
                { Minimum = min, IncludeMinimumEqual = true, Maximum = max, MinimumSet = true, MaximumSet = true };
        }

        public static OptionNotInRange<TProp> GreaterLessEqualThan<TProp>( TProp min, TProp max )
            where TProp : IComparable<TProp>
        {
            return new OptionNotInRange<TProp>
                { Minimum = min, Maximum = max, IncludeMaximumEqual = true, MinimumSet = true, MaximumSet = true };
        }
    }
}