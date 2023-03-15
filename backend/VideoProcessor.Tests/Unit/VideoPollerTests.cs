namespace VideoProcessor.Tests.Unit;

public class Tests
{
    [Test]
    public async Task VideoPoller_Detects_Video_On_Queue()
    {
        var periodicTimerMock = new Mock<IPeriodicTimerWrapper>();
        periodicTimerMock
            .Setup(m => m.WaitForNextTickAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var loggerMock = new Mock<ILogger<VideoPoller>>();

        var encodingServiceMock = new Mock<IEncodingService>();

        encodingServiceMock
            .Setup(m => m.Encode(It.IsAny<EncodingArgs>()))
            .Returns(Task.CompletedTask);

        var queueClientMock = new Mock<QueueClient>();
        var queueClientWrapperMock = new Mock<QueueClientWrapper>(queueClientMock.Object);

        var queueMessageMock = new Mock<QueueMessage>();
        var responseMock = new Mock<Response<QueueMessageWrapper>>();
        var queueMessageWrapperMock = new Mock<QueueMessageWrapper>(queueMessageMock.Object);

        var message = new VideoToProcessMessage("test.mp4", "test.mp4", "sas", false);
        var messageData = BinaryData.FromString(JsonSerializer.Serialize(message));

        queueMessageWrapperMock
            .Setup(m => m.Body)
            .Returns(messageData);

        string messageIdString = "MessageId";
        queueMessageWrapperMock
            .Setup(m => m.MessageId)
            .Returns(messageIdString);

        string popReceiptString = "PopReceipt";
        queueMessageWrapperMock
            .Setup(m => m.PopReceipt)
            .Returns(popReceiptString);

        responseMock
            .Setup(m => m.Value)
            .Returns(queueMessageWrapperMock.Object);

        queueClientWrapperMock
            .Setup(c => c.ReceiveMessageAsync())
            .ReturnsAsync(() =>
            {
                return responseMock.Object;
            });

        var deleteResponseMock = new Mock<Response>();
        deleteResponseMock
            .Setup(m => m.IsError)
            .Returns(false);

        queueClientWrapperMock
            .Setup(m => m.DeleteMessageAsync(
                It.Is<string>(messageId => messageId == messageIdString),
                It.Is<string>(popReceipt => popReceipt == popReceiptString))
            )
            .ReturnsAsync(deleteResponseMock.Object);

        var blobServiceClientMock = new Mock<BlobServiceClient>();
        var blobServiceClientWrapperMock = new Mock<BlobServiceClientWrapper>(blobServiceClientMock.Object);

        var blobContainerClientMock = new Mock<BlobContainerClient>();
        var blobContainerClientWrapperMock = new Mock<BlobContainerClientWrapper>(blobContainerClientMock.Object);

        var blobClientMock = new Mock<BlobClient>();
        var blobClientWrapperMock = new Mock<BlobClientWrapper>(blobClientMock.Object);

        blobServiceClientWrapperMock
            .Setup(m => m.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(blobContainerClientWrapperMock.Object);

        blobContainerClientWrapperMock
            .Setup(m => m.GetBlobClient(It.IsAny<string>()))
            .Returns(blobClientWrapperMock.Object);

        blobClientWrapperMock
            .Setup(m => m.DeleteAsync())
            .ReturnsAsync(deleteResponseMock.Object);

        var videoPoller = new VideoPoller(
            periodicTimerMock.Object,
            encodingServiceMock.Object,
            loggerMock.Object,
            queueClientWrapperMock.Object,
            blobServiceClientWrapperMock.Object,
            "test-raw-videos"
        );

        await videoPoller.StartAsync(CancellationToken.None);

        blobClientWrapperMock.Verify(m => m.DeleteAsync(), Times.Once());
        queueClientWrapperMock.Verify(m => m.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
    }
}