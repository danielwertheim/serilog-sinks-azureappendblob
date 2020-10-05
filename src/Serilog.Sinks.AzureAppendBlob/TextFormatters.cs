using System;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;

namespace Serilog.Sinks.AzureAppendBlob
{
    public static class TextFormatters
    {
        public static ITextFormatter CompactJson()
            => new CompactJsonFormatter();
        public static ITextFormatter Default() => CompactJson();

        public static ITextFormatter MessageTemplate(
            string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", 
            IFormatProvider formatProvider = null)
            => new MessageTemplateTextFormatter(outputTemplate, formatProvider);
    }
}