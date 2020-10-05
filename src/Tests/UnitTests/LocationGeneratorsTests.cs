using System;
using FluentAssertions;
using Serilog.Sinks.AzureAppendBlob;
using Xunit;

namespace UnitTests
{
    public class LocationGeneratorsTests
    {
        [Fact]
        public void Default_uses_logs_as_container_and_dates_as_path_and_rotates_hourly()
        {
            var dt = DateTimeOffset.UtcNow;
            var sut = LocationGenerators.Default;

            var location = sut();

            location
                .Should()
                .Be(new Location("logs", $"{dt:yyyy}/{dt:MM}/{dt:dd}/{dt:yyyyMMdd_HH}00.log"));
        }

        [Fact]
        public void Suffixed_uses_logs_as_container_and_dates_as_path_and_rotates_hourly_with_a_suffix()
        {
            var dt = DateTimeOffset.UtcNow;
            var suffix = Guid.NewGuid().ToString("N");
            var sut = LocationGenerators.Suffixed(suffix);

            var location = sut();
            
            location
                .Should()
                .Be(new Location("logs", $"{dt:yyyy}/{dt:MM}/{dt:dd}/{dt:yyyyMMdd_HH}00_{suffix}.log"));
        }
    }
}