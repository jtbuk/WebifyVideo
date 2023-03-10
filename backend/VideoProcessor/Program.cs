namespace VideoProcessor
{
    internal class Program
    {
        static void Main(string[] args)
        {   
            //TODO - Implement Upload Process
            // - Create IAC for Storage Queue
            // - Place Video On Storage Queue
            // - Read from Storage Queue
            // - Process Video
            // - Place into SMB or Blob Storage
                        
            var profile = "main"; //main or high
            var bitrate = 720;
            var audioBitrate = 360;
            var resolution = "1920x1080";
                        
            var ffmpegArguments = string.Join(" ", new string[] {
                "-i Sample/sample.mp4",
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
                "Output/sample.m3u8"
            });

            Console.WriteLine($"ffmpeg args {ffmpegArguments}");

            using var proc = new Process();
            proc.StartInfo.FileName = "/usr/bin/ffmpeg";
            proc.StartInfo.Arguments = ffmpegArguments;
            proc.Start();
        }
    }
}