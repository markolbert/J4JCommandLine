using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#pragma warning disable 8618

namespace J4JSoftware.CommandLine
{
    public class SimpleConsole : IConsoleOutput
    {
        private readonly List<string> _lines = new List<string>();

        private int _keyWidth = 20;
        private int _keyDetailSpace = 5;
        private int _detailWidth = 55;
        private string _keyDetailSep;

        public int LineWidth => KeyAreaWidth + KeyDetailSeparation + DetailAreaWidth;

        public int KeyAreaWidth
        {
            get => _keyWidth;

            set
            {
                if (value > 0)
                    _keyWidth = value;
            }
        }

        public int DetailAreaWidth
        {
            get => _detailWidth;

            set
            {
                if (value > 0)
                    _detailWidth = value;
            }
        }

        public int KeyDetailSeparation
        {
            get => _keyDetailSpace;

            set
            {
                if (value > 0)
                    _keyDetailSpace = value;
            }
        }

        public void Initialize()
        {
            _lines.Clear();
            _keyDetailSep = new string(' ', KeyDetailSeparation);
        }

        public void AddLine( ConsoleSection section, string? text = null )
        {
            _lines.Add( text ?? string.Empty );
        }

        public void AddError( CommandLineLogger.ConsolidatedLog consolidatedLog )
        {
            var keys = MergeWords( consolidatedLog.Keys, KeyAreaWidth, ", " );

            var errorLines = new List<string>();

            foreach (var error in consolidatedLog.Texts)
            {
                if (errorLines.Count > 0)
                    errorLines.Add("\n");

                errorLines.AddRange(
                    MergeWords(
                        error.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList(),
                        DetailAreaWidth
                        , " "));
            }

            OutputToConsole(keys, errorLines);

            _lines.Add(string.Empty);
        }

        public void AddOption( List<string> keys, string? description = null, string? defaultText = null )
        {
            keys = MergeWords( keys, KeyAreaWidth, ", " );

            OutputOptionBlock( keys, description ?? "*** no description provided ***" );

            if( defaultText != null )
                OutputOptionBlock( new List<string>(), $"default value: {defaultText}" );

            _lines.Add(string.Empty);
        }

        public void Display()
        {
            // the call to ToList() is done to keep xUnit, which runs tests in parallel, from
            // causing this routine to throw an InvalidOperationException because the _lines
            // collection gets modified by each separate test process
            foreach (var line in _lines.ToList())
            {
                Console.WriteLine(line);
            }

            Initialize();
        }

        private void OutputOptionBlock( List<string> keys, string text )
        {
            var optionLines = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var detail = MergeWords(optionLines, DetailAreaWidth, " ");

            OutputToConsole(keys, detail);
        }

        // takes a list of words and merges them together using the specified wordSep (e.g.,
        // a space, a ", ") such that they fit within the specified width
        private List<string> MergeWords(List<string> words, int width, string wordSep)
        {
            var retVal = new List<string>();

            var line = new StringBuilder();
            var partNum = -1;
            string? part = null;

            while (partNum < words.Count)
            {
                if (string.IsNullOrEmpty(part))
                {
                    partNum++;

                    if (partNum >= words.Count)
                        break;

                    part = words[partNum];
                }

                if (line.Length > 0)
                    line.Append(wordSep);

                while (part != null)
                {
                    var available = width - line.Length - 1;

                    if (part.Length + 1 <= available)
                    {
                        line.Append(part);
                        part = null;    // added entire part, trigger move to next part
                    }
                    else
                    {
                        if (part.Length <= width)
                        {
                            // if the entire part can fit on one line, output the
                            // current line and try to fit the part in again on
                            // a new line
                            retVal.Add(line.ToString());
                            line.Clear();
                        }
                        else
                        {
                            // part is too big for a single line, so break it
                            // up (this is arbitrary but it's simple)
                            line.Append(part.Substring(0, available));

                            part = part.Substring(available);
                        }
                    }
                }
            }

            // add remaining line to output if there is content in it
            if (line.Length > 0)
                retVal.Add(line.ToString());

            return retVal;
        }

        // outputs a block of key-focused information, either help or logger, ensuring that
        // blank lines are added when necessary to either block to make both blocks take up
        // the same number of lines
        private void OutputToConsole(List<string> keyLines, List<string> detailLines)
        {
            var numDetail = detailLines.Count;

            var maxLines = keyLines.Count;
            maxLines = maxLines > numDetail ? maxLines : numDetail;

            for (var idx = 0; idx < maxLines; idx++)
            {
                var keyArea = idx >= keyLines.Count ? string.Empty : keyLines[idx];
                keyArea = keyArea.PadRight(KeyAreaWidth);

                var detailArea = idx < detailLines.Count ? detailLines[idx] : string.Empty;
                detailArea = detailArea.PadRight(DetailAreaWidth);

                _lines.Add($"{keyArea}{_keyDetailSep}{detailArea}");
            }
        }
    }
}