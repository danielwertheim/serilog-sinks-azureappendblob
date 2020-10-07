using System;
using FluentAssertions;
using Serilog.Sinks.AzureAppendBlob;
using Xunit;

namespace UnitTests
{
    public class AzureAppendBlobBatchingOptionsTests
    {
        [Fact]
        public void Has_sensible_defaults()
        {
            var sut = AzureAppendBlobBatchingOptions.Default();

            sut.BatchSizeLimit.Should().Be(1000);
            sut.Period.Should().Be(TimeSpan.FromSeconds(15));
            sut.QueueLimit.Should().Be(10000);
        }

        [Theory]
        [InlineData(2, 2, 1000)]
        [InlineData(3, 6, 2000)]
        public void Creates_with_passed_values(int batchSizeLimit, int seconds, int queueLimit)
        {
            var sut = AzureAppendBlobBatchingOptions.Create(
                batchSizeLimit,
                TimeSpan.FromSeconds(seconds),
                queueLimit);

            sut.BatchSizeLimit.Should().Be(batchSizeLimit);
            sut.Period.Should().Be(TimeSpan.FromSeconds(seconds));
            sut.QueueLimit.Should().Be(queueLimit);
        }
    }
}