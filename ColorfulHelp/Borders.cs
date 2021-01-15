using System;

namespace J4JSoftware.Configuration.J4JCommandLine
{
    [ Flags ]
    public enum Borders
    {
        Left = 1 << 0,
        Top = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,

        None = 0,
        NoBottom = Left | Top | Right,
        NoTop = Left | Bottom | Right,
        NoLeft = Top | Bottom | Right,

        All = Left | Top | Right | Bottom
    }
}