using System;
using System.IO;
using System.Text;
using Agile.Diagnostics.Logging;

namespace Agile.Diagnostics.Loggers
{

    /// <summary>
    /// http://stackoverflow.com/questions/11911660/redirect-console-writeline-from-windows-application-to-a-string
    /// </summary>
    public class ConsoleWriterEventArgs : EventArgs
    {
        public string Value { get; }
        public ConsoleWriterEventArgs(string value)
        {
            Value = value;
        }
    }

    public class ConsoleWriter : TextWriter
    {
        private ConsoleWriter()
        {
        }

        public static void WireUpConsoleWritesToLogging()
        {
            // remove consoleLogger if it has been added (otherwise get StackOverflow)
            Logger.AllLoggers.RemoveAll(logger => logger is ConsoleLogger);

            var consoleWriter = new ConsoleWriter();

            consoleWriter.WriteEvent += (sender, args) => Logger.Info(args.Value, LogCategory.Console);
            consoleWriter.WriteLineEvent += (sender, args) => Logger.Info(args.Value, LogCategory.Console);
            Console.SetOut(consoleWriter);
            // not disposing, will be used for the lifetime of the process.
        }

        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public override void Write(string value)
        {
            WriteEvent?.Invoke(this, new ConsoleWriterEventArgs(value));
            base.Write(value);
        }

        public override void WriteLine(string value)
        {
            WriteLineEvent?.Invoke(this, new ConsoleWriterEventArgs(value));
            base.WriteLine(value);
        }

        public event EventHandler<ConsoleWriterEventArgs> WriteEvent;
        public event EventHandler<ConsoleWriterEventArgs> WriteLineEvent;
    }
}