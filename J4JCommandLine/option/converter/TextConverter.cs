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
    }
}