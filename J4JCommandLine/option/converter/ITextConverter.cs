using System;

namespace J4JSoftware.CommandLine
{
    public interface ITextConverter
    {
        Type SupportedType { get; }
        bool Convert( string value, out object result );
    }

    public interface ITextConverter<T> : ITextConverter
    {
        bool Convert( string value, out T result );
    }
}