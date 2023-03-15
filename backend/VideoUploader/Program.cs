using VideoShared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/upload", async (IFormFileCollection fileCollection) =>
{
    var mp4Files = fileCollection.Where(f => f.FileName.EndsWith(".mp4"));

    var containerName = "raw-videos";
    var storageConnection = builder.Configuration.GetConnectionString("StorageAccount");
    var blobServiceClient = new BlobServiceClient(storageConnection);
    var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
    var currentDate = DateTime.UtcNow.Ticks;

    foreach (var mp4File in mp4Files)
    {        
        var mp4Filename = $"{currentDate}-{mp4File.FileName}";
        var mp4BlobClient = blobContainerClient.GetBlobClient(mp4Filename);

        var vttFile = fileCollection.SingleOrDefault(f => f.FileName == mp4File.FileName.Replace("mp4", "vtt"));        
        if (vttFile is not null)
        {
            var vttFilename = $"{currentDate}-{vttFile.FileName}";
            var vttBlobClient = blobContainerClient.GetBlobClient(vttFilename);
            {
                await UploadFile(vttFile, vttBlobClient);
            }
        }

        await UploadFile(mp4File, mp4BlobClient);
        var sasToken = mp4BlobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddDays(1)).Query;

        var queueClient = new QueueClient(storageConnection, "raw-videos");
        await queueClient.CreateIfNotExistsAsync();

        var message = JsonSerializer.Serialize(
            new VideoToProcessMessage(
                mp4Filename,
                containerName,
                sasToken,
                HasVtt: vttFile is not null
            )
        );

        await queueClient.SendMessageAsync(message);
    }
})
.WithName("Upload")
.WithOpenApi();

app.Run();

static async Task UploadFile(IFormFile file, BlobClient blobClient)
{
    try
    {
        var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);
        stream.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

namespace VideoUploader
{
    //Necessary for testing
    public class Program { }
}

