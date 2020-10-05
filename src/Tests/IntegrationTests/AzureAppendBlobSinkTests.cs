using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Serilog.Sinks.AzureAppendBlob;
using Xunit;

namespace IntegrationTests
{
    public class AzureAppendBlobSinkTests
    {
        [Fact]
        public void Generates_container_name_according_to_defaults()
        {
            using var scenario = TestScenario.WithSinkOptions(AzureAppendBlobSinkOptions.Default());

            scenario.Log();

            scenario.ContainerClient.Exists().Value.Should().BeTrue();
            scenario.ContainerClient.Name.Should().Be("logs");
        }
        
        [Fact]
        public void Generates_blob_path_and_name_according_to_defaults()
        {
            var dt = DateTimeOffset.UtcNow;
            using var scenario = TestScenario.WithSinkOptions(AzureAppendBlobSinkOptions.Default());

            scenario.Log();

            scenario.BlobClient.Exists().Value.Should().BeTrue();
            scenario.BlobClient.Name.Should().Be($"{dt:yyyy}/{dt:MM}/{dt:dd}/{dt:yyyyMMdd_HH}00.log");
        }
        
        [Fact]
        public void Generates_container_and_blob_name_according_to_injected_generators()
        {
            var containerGuid = Guid.NewGuid().ToString("N");
            var blobNameGuid = Guid.NewGuid().ToString("N");
            using var scenario = TestScenario.WithSinkOptions(AzureAppendBlobSinkOptions.Create(
                locationGenerator: () => new Location(containerGuid, $"{blobNameGuid}.log")));

            scenario.Log();

            scenario.ContainerClient.Exists().Value.Should().BeTrue();
            scenario.ContainerClient.Name.Should().Be(containerGuid);
            
            scenario.BlobClient.Exists().Value.Should().BeTrue();
            scenario.BlobClient.Name.Should().Be($"{blobNameGuid}.log");
        }
        
        [Fact]
        public void Writes_all_logentries()
        {
            using var scenario = TestScenario.New();

            var loggedValues = scenario.LogMany(5).ToList();

            using var s = scenario.BlobClient.OpenRead();
            using var r = new StreamReader(s, Encoding.UTF8);
            foreach (var loggedValue in loggedValues) 
                r.ReadLine().Should().Contain(loggedValue);
        }
    }
}