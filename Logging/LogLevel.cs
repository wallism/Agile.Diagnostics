using System;

namespace Agile.Diagnostics.Logging
{
    [Flags]
    public enum LogLevel
    {
        None=0,
        Fatal=1,
        Error=2,
        Warning =4,
        Info=8,
        Debug = 16,
        Performance=32,
        Testing=64,
        All = 0xFFFF
    }
}