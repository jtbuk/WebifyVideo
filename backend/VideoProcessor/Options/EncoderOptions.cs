namespace VideoProcessor.Options;
public class EncoderOptions
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string TempPath { get; set; }
    public string TempOutputPath { get; set; }
    public string FFmpegPath{ get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}