namespace VideoProcessor.Features.Encoding;

public class VideoPoller : IHostedService
{
    private readonly IEncodingService _encodingService;
    private readonly IPeriodicTimerWrapper _timer;
    private readonly ILogger<VideoPoller> _logger;
    private readonly QueueClientWrapper _queueClient;
    private readonly BlobServiceClientWrapper _blobServiceClient;
    private readonly string _rawBlobContainerName;
    private bool _busy = false;

    public VideoPoller(
        IPeriodicTimerWrapper periodicTimer,
        IEncodingService encodingService,
        ILogger<VideoPoller> logger,
        QueueClientWrapper queueClient,
        BlobServiceClientWrapper blobServiceClient,
        string rawBlobContainerName
    )
    {
        _encodingService = encodingService;
        _timer = periodicTimer;
        _logger = logger;
        _queueClient = queueClient;
        _blobServiceClient = blobServiceClient;            
        _rawBlobContainerName = rawBlobContainerName;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        do
        {
            if (_busy)
            {
                _logger.LogInformation("Currently busy");
                continue;
            }

            _busy = true;
                            
            await _queueClient.CreateIfNotExistsAsync();

            var message = await _queueClient.ReceiveMessageAsync();
            

            if (message is null)
            {
                _logger.LogInformation("No messages in the queue");
                _busy = false;
                continue;
            }
            
            var videoToProcessMessage = JsonSerializer.Deserialize<VideoToProcessMessage>(message.Body.ToString())!;
                            
            var containerClient = _blobServiceClient.GetBlobContainerClient(_rawBlobContainerName);
            var blobClient = containerClient.GetBlobClient(videoToProcessMessage.VideoFilename);

            byte[] contents;
            using (var memoryStream = new MemoryStream())
            {
                blobClient.DownloadTo(memoryStream);
                contents = memoryStream.ToArray();
            }

            var encodingArgs = new EncodingArgs(videoToProcessMessage.VideoFilename, videoToProcessMessage.HasVtt, contents);

            await _encodingService.Encode(encodingArgs);

            var deleteMessageResponse = await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            if (deleteMessageResponse.IsError)
            {
                _logger.LogError("Failed to delete storage queue message");
            }

            var deleteBlobResponse = await blobClient.DeleteAsync();
            if (deleteBlobResponse.IsError)
            {
                _logger.LogError("Failed to delete blob");
            }

            _busy = false;
        }
        while (await _timer.WaitForNextTickAsync(cancellationToken));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}