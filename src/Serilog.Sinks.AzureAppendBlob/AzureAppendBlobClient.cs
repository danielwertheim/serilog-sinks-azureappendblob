using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Serilog.Sinks.AzureAppendBlob
{
    public interface IAzureAppendBlobSinkClient
    {
        Task AppendAsync(Location location, Stream stream);
    }

    internal class AzureAppendBlobClient : IAzureAppendBlobSinkClient
    {
        private readonly BlobServiceClient _blobServiceClient;

        private BlobContainerClient _containerClient;
        private AppendBlobClient _blobClient;

        private string _lastContainerName;
        private string _lastBlobName;

        internal AzureAppendBlobClient(
            BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        }
        
        private async Task RefreshContainerAndBlobAsync(Location location, long blobPayloadSize)
        {
            var newContainerName = location.ContainerName;
            if (_containerClient == null ||
                !string.Equals(_lastContainerName, newContainerName))
            {
                _containerClient = _blobServiceClient.GetBlobContainerClient(newContainerName);
                await _containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
                _lastContainerName = newContainerName;
            }

            //TODO: Use blobPayloadSize
            // var blobInfo = await _blobClient.GetPropertiesAsync().ConfigureAwait(false);
            // var newSize = blobInfo.Value.ContentLength + blobPayloadSize;
            // if (newSize > _blobClient.AppendBlobMaxAppendBlockBytes)
            // {
            //     
            // }

            var newBlobName = location.BlobName;
            if (_blobClient == null ||
                !string.Equals(_lastBlobName, newBlobName))
            {
                _blobClient = _containerClient.GetAppendBlobClient(newBlobName);
                await _blobClient.CreateIfNotExistsAsync().ConfigureAwait(false);
                _lastBlobName = newBlobName;
            }
        }

        public async Task AppendAsync(Location location, Stream stream)
        {
            if(stream == null)
                throw new ArgumentNullException(nameof(stream));
            
            if(location == null)
                throw new ArgumentNullException(nameof(location));
            
            if(stream == Stream.Null)
                return;
            
            await RefreshContainerAndBlobAsync(location, stream.Length).ConfigureAwait(false);

            await _blobClient.AppendBlockAsync(stream).ConfigureAwait(false);
        }
    }

    public static class BlobServiceExtensions
    {
        public static IAzureAppendBlobSinkClient ToAppendBlobClient(this BlobServiceClient client)
            => new AzureAppendBlobClient(client);
    }
}