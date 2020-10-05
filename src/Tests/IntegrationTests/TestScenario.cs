using System;
using System.Collections.Generic;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Serilog;
using Serilog.Sinks.AzureAppendBlob;

namespace IntegrationTests
{
    public class TestScenario : IDisposable
    {
        private readonly AzureAppendBlobSinkOptions _sinkOptions;
        private readonly IAzureAppendBlobSinkClient _azureAppendBlobSinkClient;
        public BlobContainerClient ContainerClient { get; }
        public AppendBlobClient BlobClient { get; }

        private TestScenario(
            AzureAppendBlobSinkOptions sinkOptions)
        {
            _sinkOptions = sinkOptions ?? throw new ArgumentNullException(nameof(sinkOptions));
            var blobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
            _azureAppendBlobSinkClient = blobServiceClient.ToAppendBlobClient();

            var location = _sinkOptions.LocationGenerator();
            ContainerClient = blobServiceClient.GetBlobContainerClient(location.ContainerName);
            BlobClient = ContainerClient.GetAppendBlobClient(location.BlobName);
        }

        public static TestScenario New()
        {
            var scenarioId = Guid.NewGuid().ToString("N");

            return new TestScenario(AzureAppendBlobSinkOptions.Create(
                locationGenerator: () => new Location($"test_logs_{scenarioId}", $"{scenarioId}.log")));
        }

        public static TestScenario WithSinkOptions(AzureAppendBlobSinkOptions options)
            => new TestScenario(options);

        public string Log()
        {
            using var logger = new LoggerConfiguration()
                .WriteTo.AzureAppendBlob(
                    _azureAppendBlobSinkClient,
                    _sinkOptions)
                .CreateLogger();

            var logValue = Guid.NewGuid().ToString("N");
            logger.Information(logValue);
            return logValue;
        }

        public IEnumerable<string> LogMany(int n = 2)
        {
            using var logger = new LoggerConfiguration()
                .WriteTo.AzureAppendBlob(
                    _azureAppendBlobSinkClient,
                    _sinkOptions)
                .CreateLogger();

            for (var c = 0; c < n; c++)
            {
                var logValue = Guid.NewGuid().ToString("N");
                logger.Information(logValue);
                yield return logValue;
            }
        }

        public void Dispose() => ContainerClient?.DeleteIfExists();
    }
}