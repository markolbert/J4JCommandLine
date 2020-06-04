using System;
using Alba.CsConsoleFormat;

namespace J4JSoftware.CommandLine
{
    public interface IFancyOutputConfiguration
    {
        bool FrameErrors { get; set; }
        bool FrameHelp { get; set; }
        bool ShowGrid { get; set; }
        ConsoleColor TitleColor { get; set; }
        ConsoleColor ErrorColor { get; set; }
        ConsoleColor HelpColor { get; set; }
        ConsoleColor GridColor { get; set; }
        LineThickness GridThickness { get; set; }
        Align HeaderAlignment { get; set; }
        Align KeyAlignment { get; set; }
        Thickness Margin { get; set; }
    }
}