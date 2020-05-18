using System;

namespace J4JSoftware.CommandLine
{
    public abstract class TextConverter<T> : ITextConverter<T>
    {
        protected TextConverter()
        {
        }

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