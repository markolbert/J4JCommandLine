﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine.Deprecated
{
    // determines whether an object is not contained in a pre-defined list of objects. This is done
    // by using the object's IEquatable<T> interface, so only Types supporting that interface
    // can be validated by this object.
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

        public override bool Validate( Option option, T value, CommandLineLogger logger )
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

            logger.LogError( ProcessingPhase.Validating, $"'{value}' is in set '{validValues}'", option: option );

            return false;
        }
    }
}