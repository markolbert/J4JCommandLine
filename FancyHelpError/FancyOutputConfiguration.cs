using System;
using Alba.CsConsoleFormat;

namespace J4JSoftware.CommandLine
{
    public class FancyOutputConfiguration //: OutputConfiguration, IFancyOutputConfiguration
    {
        public int MaxWidth { get; set; } = 80;
        public bool FrameErrors { get; set; }
        public bool FrameHelp { get; set; }
        public bool ShowGrid { get; set; }
        public ConsoleColor TitleColor { get; set; } = ConsoleColor.Gray;
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public ConsoleColor HelpColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor GridColor { get; set; } = ConsoleColor.Gray;

        public LineThickness GridThickness { get; set; }
            = new LineThickness( Alba.CsConsoleFormat.LineWidth.Single, Alba.CsConsoleFormat.LineWidth.Single );

        public Align HeaderAlignment { get; set; } = Align.Center;
        public Align KeyAlignment { get; set; } = Align.Center;
        public Thickness Margin { get; set; } = new Thickness(1,1,1,1);
    }
}