namespace VideoShared.AzureSdkWrappers;

public class QueueClientWrapper
{
    private readonly QueueClient _queueClient;

    public QueueClientWrapper(QueueClient queueClient)
    {
        _queueClient = queueClient;

    }

    public async virtual Task CreateIfNotExistsAsync()
    {
        await _queueClient.CreateIfNotExistsAsync();
    }

    public async virtual Task<Response> DeleteMessageAsync(string messageId, string popReceipt)
    {
        return await _queueClient.DeleteMessageAsync(messageId, popReceipt);
    }

    public async virtual Task<QueueMessageWrapper> ReceiveMessageAsync(TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        var message = await _queueClient.ReceiveMessageAsync(timeSpan, cancellationToken);

        return new(message.Value);
    }

    public async virtual Task<QueueMessageWrapper> ReceiveMessageAsync()
    {
        var message = await _queueClient.ReceiveMessageAsync();

        return new(message.Value);
    }
}

public class ResponseWrapper<T>
{
    private readonly Response<T> _response;

    public ResponseWrapper(Response<T> response)
    {
        _response = response;
    }
}

public class QueueMessageWrapper
{
    private readonly QueueMessage _queueMessage;

    public QueueMessageWrapper(QueueMessage queueMessage)
    {
        _queueMessage = queueMessage;
    }

    public virtual BinaryData Body => _queueMessage.Body;
    public virtual string MessageId => _queueMessage.MessageId;
    public virtual string PopReceipt => _queueMessage.PopReceipt;
}