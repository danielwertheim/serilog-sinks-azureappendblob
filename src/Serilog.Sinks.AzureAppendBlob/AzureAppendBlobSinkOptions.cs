using System;
using Serilog.Formatting;

namespace Serilog.Sinks.AzureAppendBlob
{
    public class AzureAppendBlobSinkOptions
    {
        public ITextFormatter Formatter { get; }
        public LocationGenerator LocationGenerator { get; }

        private AzureAppendBlobSinkOptions(
            ITextFormatter formatter,
            LocationGenerator locationGenerator)
        {
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            LocationGenerator = locationGenerator ?? throw new ArgumentNullException(nameof(LocationGenerator));
        }

        public static AzureAppendBlobSinkOptions Create(
            ITextFormatter formatter = null,
            LocationGenerator locationGenerator = null) 
            => new AzureAppendBlobSinkOptions(
                formatter ?? TextFormatters.Default(),
                locationGenerator ?? LocationGenerators.Default);

        public static AzureAppendBlobSinkOptions Default() => Create();
    }
}