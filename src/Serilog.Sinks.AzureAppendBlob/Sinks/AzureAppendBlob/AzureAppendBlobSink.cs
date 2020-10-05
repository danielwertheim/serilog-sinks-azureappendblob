using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AzureAppendBlob.Sinks.AzureAppendBlob
{
    internal class AzureAppendBlobSink : IBatchedLogEventSink
    {
        private static readonly UTF8Encoding WriterEncoding = new UTF8Encoding(false);
        
        private readonly AzureAppendBlobSinkOptions _options;
        private readonly IAzureAppendBlobSinkClient _client;

        internal AzureAppendBlobSink(
            AzureAppendBlobSinkOptions options,
            IAzureAppendBlobSinkClient client)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task OnEmptyBatchAsync()
            => EmitBatchAsync(Enumerable.Empty<LogEvent>());

        public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            if (batch == null)
                throw new ArgumentNullException(nameof(batch));

            using var e = batch.GetEnumerator();

            while (e.MoveNext())
            {
#if NETSTANDARD2_1
                await using var stream = new MemoryStream();
                await using var writer = new StreamWriter(stream, WriterEncoding, -1, true);
#else
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream, WriterEncoding, -1, true);
#endif
                _options.Formatter.Format(e.Current, writer);

                while (e.MoveNext())
                    _options.Formatter.Format(e.Current, writer);

                // ReSharper disable once MethodHasAsyncOverload
                writer.Flush();

                stream.Position = 0;

                await _client.AppendAsync(
                    _options.LocationGenerator(),
                    stream).ConfigureAwait(false);

                break;
            }
        }
    }
}