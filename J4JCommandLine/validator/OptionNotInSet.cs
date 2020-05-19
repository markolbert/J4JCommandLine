﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class OptionNotInSet<T> : OptionValidator<T>
        where T : IEquatable<T>
    {
        private readonly List<T> _checkValues;

        public OptionNotInSet( params T[] checkValues )
        {
            _checkValues = new List<T>( checkValues );
        }

        public OptionNotInSet( List<T> checkValues )
        {
            _checkValues = checkValues;
        }

        public override bool Validate( IBindingTarget bindingTarget, string key, T value )
        {
            if( !_checkValues.Any( v => v.Equals( value ) ) )
                return true;

            var validValues = _checkValues.Aggregate(
                new StringBuilder(),
                ( sb, x ) =>
                {
                    if( sb.Length > 0 )
                        sb.Append( ", " );

                    return sb.Append( x );
                },
                sb => sb.ToString() );

            bindingTarget.AddError( key, $"'{value}' is in set '{validValues}'" );

            return false;
        }
    }
}