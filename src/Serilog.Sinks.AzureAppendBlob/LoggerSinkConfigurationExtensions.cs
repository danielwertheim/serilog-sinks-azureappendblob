using System;
using Azure.Storage.Blobs;
using Serilog.Configuration;
using Serilog.Sinks.AzureAppendBlob.Sinks.AzureAppendBlob;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AzureAppendBlob
{
    public static class LoggerSinkConfigurationExtensions
    {
        public static LoggerConfiguration AzureAppendBlob(this LoggerSinkConfiguration sinkConfiguration,
            IAzureAppendBlobSinkClient azureAppendBlobSinkClient,
            AzureAppendBlobSinkOptions sinkOptions = null,
            AzureAppendBlobBatchingOptions batchingOptions = null)
        {
            if (azureAppendBlobSinkClient == null)
                throw new ArgumentNullException(nameof(azureAppendBlobSinkClient));

            sinkOptions ??= AzureAppendBlobSinkOptions.Default();
            batchingOptions ??= AzureAppendBlobBatchingOptions.Default();

            var batchingSink = new AzureAppendBlobSink(
                sinkOptions,
                azureAppendBlobSinkClient);

            var sink = new PeriodicBatchingSink(
                batchingSink,
                batchingOptions.ToSerilogBatchingOptions());

            return sinkConfiguration.Sink(sink);
        }
    }
}