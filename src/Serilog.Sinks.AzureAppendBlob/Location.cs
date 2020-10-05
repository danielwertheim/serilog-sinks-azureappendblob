using System;
using System.Runtime.CompilerServices;

namespace Serilog.Sinks.AzureAppendBlob
{
    public delegate Location LocationGenerator();

    public static class LocationGenerators
    {
        /// <summary>
        /// Generates a container name "logs" and blob name with a path and
        /// name consiting of UTC year, month, day. The name also contains
        /// hours (24hour format), meaning logs will be rotated each hour.
        /// </summary>
        /// <example>logs/2020/07/03/20200703_13.log</example>
        public static readonly LocationGenerator Default =
            () =>
            {
                var dt = DateTimeOffset.UtcNow;
                
                return new Location(
                    "logs", 
                    $"{dt:yyyy}{Location.PathDivider}{dt:MM}{Location.PathDivider}{dt:dd}{Location.PathDivider}{dt:yyyyMMdd_HH}00.log");
            };

        /// <summary>
        /// Generates a container name "logs" and blob name with a path and
        /// name consiting of UTC year, month, day. The name also contains
        /// hours (24hour format), meaning logs will be rotated each hour.
        /// </summary>
        /// <example>logs/2020/07/03/20200703_13.log</example>
        public static LocationGenerator Suffixed(string suffix)
        {
            return () =>
            {
                var dt = DateTimeOffset.UtcNow;

                return new Location(
                    "logs",
                    $"{dt:yyyy}{Location.PathDivider}{dt:MM}{Location.PathDivider}{dt:dd}{Location.PathDivider}{dt:yyyyMMdd_HH}00_{suffix}.log");
            };
        }
    }

    public sealed class Location : IEquatable<Location>
    {
        public const string PathDivider = "/";

        public string ContainerName { get; }
        public string BlobName { get; }

        public Location(string containerName, string blobName)
        {
            containerName = containerName?.Trim('/', '\\');
            blobName = blobName?.Trim('/', '\\');

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Container name must be provided.", nameof(containerName));

            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob name must be provided.", nameof(blobName));

            ContainerName = containerName;
            BlobName = blobName;
        }

        public bool Equals(Location other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                string.Equals(
                    ContainerName,
                    other.ContainerName,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    BlobName,
                    other.BlobName,
                    StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
            => ReferenceEquals(this, obj) || obj is Location other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(ContainerName) * 397 ^
                       StringComparer.OrdinalIgnoreCase.GetHashCode(BlobName);
            }
        }

        public static bool operator ==(Location left, Location right)
            => Equals(left, right);

        public static bool operator !=(Location left, Location right)
            => !Equals(left, right);

        public override string ToString() => $"{ContainerName}{PathDivider}{BlobName}";
    }
}