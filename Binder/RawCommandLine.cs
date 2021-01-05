using System;
using System.Runtime.InteropServices;

namespace J4JSoftware.Configuration.CommandLine
{
    public class RawCommandLine 
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetCommandLine();

        private readonly bool _useKernel32;

        public RawCommandLine()
        {
            _useKernel32 = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => true,
                PlatformID.Win32S => true,
                PlatformID.Win32Windows => true,
                PlatformID.WinCE => true,
                _ => false
            };
        }

        public string GetRawCommandLine()
        {
            if( !_useKernel32 ) 
                return Environment.CommandLine;

            IntPtr ptr = GetCommandLine();

            return Marshal.PtrToStringAuto( ptr )!;
        }
    }
}