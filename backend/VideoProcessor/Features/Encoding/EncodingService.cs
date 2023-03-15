namespace VideoProcessor.Features.Encoding;

public record EncodingArgs(string Filename, bool HasVtt, byte[] Content);

public interface IEncodingService
{
    Task Encode(EncodingArgs encodingArgs);
}

public class EncodingService : IEncodingService
{
    private readonly EncoderOptions _encoderOptions;

    public EncodingService(IOptions<EncoderOptions> encoderOptions)
    {
        _encoderOptions = encoderOptions.Value;
    }

    public async Task Encode(EncodingArgs encodingArgs)
    {
        //Write the file to a temp directory ready for ffmpeg to pick up
        var inputFile = Path.Combine(_encoderOptions.TempPath, encodingArgs.Filename);
        var outputFile = Path.Combine(_encoderOptions.TempOutputPath, encodingArgs.Filename.Replace("mp4", "m3u8"));

        Directory.CreateDirectory(_encoderOptions.TempPath);
        Directory.CreateDirectory(_encoderOptions.TempOutputPath);

        await File.WriteAllBytesAsync(inputFile, encodingArgs.Content);

        RunFFmpeg(inputFile, outputFile, encodingArgs.HasVtt);
    }

    private void RunFFmpeg(string inputFile, string outputFile, bool hasVtt)
    {
        var profile = "main"; //main or high
        var bitrate = 720;
        var audioBitrate = 360;
        var resolution = "1920x1080";

        //TODO - Deal with subtitles
        var vttFile = inputFile.Replace("mp4", "vtt");

        var ffmpegArguments = string.Join(" ", new string[] {
            $"-i {inputFile}",
            "-vcodec h264",
            "-acodec aac",
            "-force_key_frames expr:gte(t,n_forced*5)",
            $"-profile:v {profile}",
            "-preset veryfast",
            "-crf 22",
            $"-maxrate {bitrate}",
            $"-bufsize {bitrate * 2}",
            $"-ab {audioBitrate}",
            $"-s {resolution}",
            "-movflags +faststart",
            //Must be last
            $"{outputFile}"
        });

        Console.WriteLine($"ffmpeg args {ffmpegArguments}");

        using var proc = new Process();
        proc.StartInfo.FileName = _encoderOptions.FFmpegPath;
        proc.StartInfo.Arguments = ffmpegArguments;
        proc.Start();
    }
}