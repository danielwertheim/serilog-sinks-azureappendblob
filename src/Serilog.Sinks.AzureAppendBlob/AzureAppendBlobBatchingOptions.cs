using System;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AzureAppendBlob
{
    public class AzureAppendBlobBatchingOptions
    {
        public int BatchSizeLimit { get; }
        public TimeSpan Period { get; }
        public int QueueLimit { get; }

        private AzureAppendBlobBatchingOptions(
            int batchSizeLimit,
            TimeSpan period,
            int queueLimit)
        {
            if (batchSizeLimit < 2)
                throw new ArgumentOutOfRangeException(nameof(batchSizeLimit), "Must be at least 2.");

            if (period.TotalSeconds < 2)
                throw new ArgumentOutOfRangeException(nameof(period), "Must be at least 2 seconds.");

            if (queueLimit < 1000)
                throw new ArgumentOutOfRangeException(nameof(queueLimit), "Must be at least 1000.");

            BatchSizeLimit = batchSizeLimit;
            Period = period;
            QueueLimit = queueLimit;
        }
        
        public static AzureAppendBlobBatchingOptions Create(
            int batchSizeLimit = 1000,
            TimeSpan? period = null,
            int queueLimit= 10000) 
            => new AzureAppendBlobBatchingOptions(
                batchSizeLimit,
                period ?? TimeSpan.FromSeconds(15),
                queueLimit);

        public static AzureAppendBlobBatchingOptions Default() => Create();

        internal PeriodicBatchingSinkOptions ToSerilogBatchingOptions()
            => new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = BatchSizeLimit,
                Period = Period,
                QueueLimit = QueueLimit
            };
    }
}