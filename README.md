# Serilog-Sinks-AzureAppendBlob
Serilog sink that logs to append blobs in an Azure Storage Account. This is not an offican sink, but a community driven one. This also has nothing todo with the already existing sink for Azure blobs [serilog-sinks-azureblobstorage](https://github.com/chriswill/serilog-sinks-azureblobstorage) which I've previously used myself with success.

It currently targets `netstandard2.0` and `netstandard2.1`. Feel free to raise an issue if that is not enough.

## So why a new Sink?
Because I needed the new SDK ([Azure.Storage.Blobs](https://www.nuget.org/packages/Azure.Storage.Blobs)) for working with Azure Blobs and not the older one used by the previous mentioned Sink. This so that I could make used of managed user identities etc in Azure in a simple way using [Azure.Identity](https://www.nuget.org/packages/Azure.Identity). Yes, I could have contributed but I also wanted to take another approach in e.g. naming containers and blobs by instead having the user inject functions that control this behavior. I also wanted to accept a custom client defintion `IAzureAppendBlobSinkClient` so that users potentially just could inject their own implementation for providing custom behavior around e.g the stream etc.

## Sequencing
As of now there's no logic builtin for sequencing new logfiles if the maximum is reached. By default the log rolls per-hour so hitting the (maximum of 195GiB)[https://docs.microsoft.com/en-us/rest/api/storageservices/understanding-block-blobs--append-blobs--and-page-blobs#about-append-blobs] per hour... Well, let me know and we will come up with something. The plan is to look at the return codes when appending to see if the error indicates that a maximum has been exceeded and then to generate a new name.

## Testing
Just spin up the `docker-compose` file and you are good to go. It uses [Azurite](https://github.com/Azure/Azurite) to host a append blob compatible blob storage.

UnitTests runs with no side dependencies (as they should) while integration tests uses a connection string: `UseDevelopmentStorage=true`; hence why Azurite is needed (see info about `docker-compose` above).

## Usage
The minimum you need to pass is an instance of `IAzureAppendBlobSinkClient`. There's an extension method provided for you so that you can turn a `BlobServiceClient` to an default implementation of the custom interface:

```csharp
var client = new BlobServiceClient("some_cn_string").ToAppendBlobClient();
``` 

Use the client when configuring the sink:

```csharp
var logger = new LoggerConfiguration()
    .WriteTo.AzureAppendBlob(client)
    .CreateLogger();
```

### AzureAppendBlobSinkOptions
The options available for configuring the behavior of the Sink is provided via an immutable options class: `AzureAppendBlobSinkOptions`.

```csharp
var logger = new LoggerConfiguration()
    .WriteTo.AzureAppendBlob(
        client,
        AzureAppendBlobSinkOptions.Create())
    .CreateLogger();
```

The defaults it uses is as follows:

```csharp
var logger = new LoggerConfiguration()
    .WriteTo.AzureAppendBlob(
        client,
        AzureAppendBlobSinkOptions.Create(
            formatter: TextFormatters.CompactJson(),
            locationGenerator: LocationGenerators.Default))
    .CreateLogger();
```

#### TextFormatters
By default it uses the `CompactJsonFormatter` provided via [Serilog.Formatting.Compact](https://www.nuget.org/packages/Serilog.Formatting.Compact) but you can of course provide your own:

```csharp
var logger = new LoggerConfiguration()
    .WriteTo.AzureAppendBlob(
        client,
        AzureAppendBlobSinkOptions.Create(
            formatter: myTextFormatterOfChoice))
    .CreateLogger();
```

There's a helper for using `MessageTemplateTextFormatter` via `TextFormatters.MessageTemplate(template, provider)`

```csharp
var logger = new LoggerConfiguration()
    .WriteTo.AzureAppendBlob(
        client,
        AzureAppendBlobSinkOptions.Create(
            formatter: TextFormatters.MessageTemplate(
                optionalTemplate, 
                optionalProvider)))
    .CreateLogger();
```

#### LocationGenerators
You can pass in what-ever function you want, as long as it takes no arguments and returns a `Location`. Shipped generators are:

- `LocationGenerators.Default`:
    - ContainerName: `logs`
    - BlobName: `/yyyy/MM/dd/yyyyMMdd_HH00.log`
- `LocationGenerators.Suffixed("mySuffix")`
    - ContainerName: `logs`
    - BlobName: `/yyyy/MM/dd/yyyyMMdd_HH00_mySuffix.log`

The sample below mimics the default gemerator.

```csharp
var locationGenerator = () =>
{
    var dt = DateTimeOffset.UtcNow;
    
    return new Location(
        "logs", 
        $"{dt:yyyy}{Location.PathDivider}{dt:MM}{Location.PathDivider}{dt:dd}{Location.PathDivider}{dt:yyyyMMdd_HH}00.log");
};

var logger = new LoggerConfiguration()
    .WriteTo.AzureAppendBlob(
        client,
        AzureAppendBlobSinkOptions.Create(
            locationGenerator: locationGenerator)))
    .CreateLogger();
```

### AzureAppendBlobBatchingOptions
It uses the `PeriodicBatchingSink` provided via [Serilog.Sinks.PeriodicBatching](https://www.nuget.org/packages/Serilog.Sinks.PeriodicBatching) so you can pass specific values for controlling the batching behavior. The defaults are shown below.

```csharp
var logger = new LoggerConfiguration()
    .WriteTo.AzureAppendBlob(
        client,
        batchingOptions: AzureAppendBlobBatchingOptions.Create(
            batchSizeLimit: 1000,
            TimeSpan.FromSeconds(15),
            queueLimit: 10000))
    .CreateLogger();
```