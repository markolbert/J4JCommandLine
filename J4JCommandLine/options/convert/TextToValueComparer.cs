using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine;

public sealed class TextToValueComparer : IEqualityComparer<ITextToValue>
{
    public bool Equals( ITextToValue? x, ITextToValue? y )
    {
        if ( ReferenceEquals( x, y ) ) return true;
        if ( ReferenceEquals( x, null ) ) return false;
        if ( ReferenceEquals( y, null ) ) return false;
        if ( x.GetType() != y.GetType() ) return false;

        return x.TargetType.Equals( y.TargetType );
    }

    public int GetHashCode( ITextToValue obj )
    {
        return obj.TargetType.GetHashCode();
    }
}
