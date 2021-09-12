using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class TextToEnum<TEnum> : TextToValue<TEnum>
        where TEnum: Enum
    {
        public TextToEnum( IJ4JLogger? logger ) 
            : base( Customization.BuiltIn, Int32.MinValue, logger )
        {
        }

        protected override bool ConvertTextToValue( string text, out TEnum? result )
        {
            result = default(TEnum);

            if( !Enum.TryParse( typeof(TEnum), text, true, out var tempResult ) )
                return false;

            result = (TEnum?)tempResult;

            return true;
        }
    }
}
