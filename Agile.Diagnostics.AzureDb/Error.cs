using System;

namespace Agile.Diagnostics.AzureDb
{
    public class Error
    {
        public string Message { get; set; }

        public string Machine { get; set; }

        public int? AppKey { get; set; }

        public string App { get; set; }

        public string Component { get; set; }

        public string Version { get; set; }

        public string ThreadId { get; set; }

        public DateTimeOffset Created { get; set; }

        public string ExType { get; set; }
    }
}