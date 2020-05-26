using System;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public partial class OptionInRange<T> : OptionValidator<T>
        where T : IComparable<T>
    {
        public static OptionInRange<TProp> GreaterThan<TProp>( TProp min ) where TProp : IComparable<TProp>
        {
            return new OptionInRange<TProp> { Minimum = min, MinimumSet = true };
        }

        public static OptionInRange<TProp> GreaterThanEqual<TProp>( TProp min ) where TProp : IComparable<TProp>
        {
            return new OptionInRange<TProp> { Minimum = min, IncludeMinimumEqual = true, MinimumSet = true };
        }

        public static OptionInRange<TProp> LessThan<TProp>( TProp max ) where TProp : IComparable<TProp>
        {
            return new OptionInRange<TProp> { Maximum = max, MaximumSet = true };
        }

        public static OptionInRange<TProp> LessThanEqual<TProp>( TProp max ) where TProp : IComparable<TProp>
        {
            return new OptionInRange<TProp> { Maximum = max, IncludeMaximumEqual = true, MaximumSet = true };
        }

        public static OptionInRange<TProp> GreaterLessThan<TProp>( TProp min, TProp max )
            where TProp : IComparable<TProp>
        {
            return new OptionInRange<TProp> { Minimum = min, Maximum = max, MinimumSet = true, MaximumSet = true };
        }

        public static OptionInRange<TProp> GreaterLessThanEqual<TProp>( TProp min, TProp max )
            where TProp : IComparable<TProp>
        {
            return new OptionInRange<TProp>
            {
                Minimum = min, IncludeMinimumEqual = true, Maximum = max, IncludeMaximumEqual = true, MinimumSet = true,
                MaximumSet = true
            };
        }

        public static OptionInRange<TProp> GreaterEqualLessThan<TProp>( TProp min, TProp max )
            where TProp : IComparable<TProp>
        {
            return new OptionInRange<TProp>
                { Minimum = min, IncludeMinimumEqual = true, Maximum = max, MinimumSet = true, MaximumSet = true };
        }

        public static OptionInRange<TProp> GreaterLessEqualThan<TProp>( TProp min, TProp max )
            where TProp : IComparable<TProp>
        {
            return new OptionInRange<TProp>
                { Minimum = min, Maximum = max, IncludeMaximumEqual = true, MinimumSet = true, MaximumSet = true };
        }
    }
}