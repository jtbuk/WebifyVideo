namespace VideoShared;

public record VideoToProcessMessage(string VideoFilename, string ContainerName, string SasToken, bool HasVtt);