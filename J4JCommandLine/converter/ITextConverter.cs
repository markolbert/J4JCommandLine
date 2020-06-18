using System;

namespace J4JSoftware.CommandLine
{
    // The non-generic interface defining how a text value gets converted to a 
    // particular type (SupportedType) of object
    public interface ITextConverter
    {
        Type SupportedType { get; }
        bool Convert( string value, out object result );
    }

    // The generic interface corresponding to the non-generic interface defined above
    public interface ITextConverter<T> : ITextConverter
    {
        bool Convert( string value, out T result );
    }
}