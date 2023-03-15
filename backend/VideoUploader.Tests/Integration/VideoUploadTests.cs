namespace VideoUploader.Tests.Integration;
public class Tests
{
    [Test]
    public async Task VideoUpload_Succeeds()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        var assembly = Assembly.GetExecutingAssembly();

        using var fileStream = assembly.GetManifestResourceStream("VideoUploader.Tests.Sample.sample.mp4")!;
        using var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        var fileBytes = memoryStream.ToArray();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

        using var vttFileStream = assembly.GetManifestResourceStream("VideoUploader.Tests.Sample.sample.vtt")!;
        using var vttMemoryStream = new MemoryStream();
        vttFileStream.CopyTo(vttMemoryStream);
        var vttFileBytes = vttMemoryStream.ToArray();
        var vttFileContent = new ByteArrayContent(fileBytes);
        vttFileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

        var formIdContent = new StringContent("form_id");
        var titleContent = new StringContent("some title");

        var httpContent = new MultipartFormDataContent
        {
            { fileContent, "video_file", "sample.mp4" },
            { vttFileContent, "vtt_file", "sample.vtt" },
            { formIdContent, "id" },
            { titleContent, "title" }
        };

        var response = await client.PostAsync("/upload", httpContent);
        response.EnsureSuccessStatusCode();
    }
}