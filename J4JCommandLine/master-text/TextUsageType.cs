﻿namespace J4JSoftware.Configuration.CommandLine
{
    // the various ways in which a piece of text can be used within
    // the framework
    public enum TextUsageType
    {
        Prefix,
        Quote,
        ValueEncloser,
        OptionKey,
        Separator,
        Undefined
    }
}