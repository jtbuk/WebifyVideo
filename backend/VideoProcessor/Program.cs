using VideoShared.NetWrappers;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        var encoderSection = configuration.GetSection("EncoderOptions");
        services
            .AddOptions<EncoderOptions>()
            .Bind(encoderSection)
            .Validate((options) => !string.IsNullOrEmpty(options.TempPath));

        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<IEncodingService, EncodingService>();        
        services.AddHostedService(serviceProvider =>
        {
            var encodingService = serviceProvider.GetRequiredService<EncodingService>();
            var videoPollerLogger = serviceProvider.GetRequiredService<ILogger<VideoPoller>>();
            var storageConnection = configuration.GetConnectionString("StorageAccount")!;
            
            var storageQueueName = "raw-videos";
            var blobStorageContainerName = "raw-videos";

            return new VideoPoller (
                new PeriodicTimerWrapper(new PeriodicTimer(TimeSpan.FromSeconds(10))),
                encodingService,
                videoPollerLogger,
                new (new QueueClient(storageConnection, storageQueueName)),
                new (new BlobServiceClient(storageConnection)),                
                blobStorageContainerName
            );
        });
    })
    .Build();

await host.RunAsync();