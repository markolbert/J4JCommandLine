using System;

namespace J4JSoftware.CommandLine.Deprecated
{
    // An abstract base class implementing the ITextConverter<T> and ITextConverter
    // interfaces.
    public abstract class TextConverter<T> : ITextConverter<T>
    {
        public Type SupportedType => typeof(T);
        public abstract bool Convert( string value, out T result );

        bool ITextConverter.Convert( string value, out object result )
        {
            var retVal = Convert( value, out var innerResult );

            result = innerResult!;

            return retVal;
        }
    }
}