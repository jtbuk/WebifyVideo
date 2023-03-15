namespace VideoShared.AzureSdkWrappers;

public class BlobServiceClientWrapper
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobServiceClientWrapper(BlobServiceClient blobServiceClient)
	{
        _blobServiceClient = blobServiceClient;
    }

    public virtual BlobContainerClientWrapper GetBlobContainerClient(string blobContainerName)
    {
        return new(_blobServiceClient.GetBlobContainerClient(blobContainerName));
    }
}

public class BlobContainerClientWrapper
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobContainerClientWrapper(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public virtual BlobClientWrapper GetBlobClient(string blobName)
    {
        return new(_blobContainerClient.GetBlobClient(blobName));
    }
}

public class BlobClientWrapper
{
    private readonly BlobClient _blobClient;

    public BlobClientWrapper(BlobClient blobClient)
    {
        _blobClient = blobClient;
    }

    public virtual async Task<Response> DeleteAsync()
    {
        return await _blobClient.DeleteAsync();
    }

    public virtual Response DownloadTo(Stream stream)
    {
        return _blobClient.DownloadTo(stream);
    }
}