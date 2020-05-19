using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class TextToEnum<TEnum> : TextConverter<TEnum>
        where TEnum : Enum
    {
        public override bool Convert( string value, out TEnum result )
        {
            if( Enum.TryParse( typeof(TEnum), value, out var innerResult )
                && innerResult != null )
            {
                result = (TEnum) innerResult;

                return true;
            }

            // find the least value of TEnum
            var values = new List<object>();

            foreach( var enumValue in Enum.GetValues( typeof(TEnum) ) )
                if( enumValue != null )
                    values.Add( enumValue );

            result = (TEnum) values.Min();

            return false;
        }
    }
}