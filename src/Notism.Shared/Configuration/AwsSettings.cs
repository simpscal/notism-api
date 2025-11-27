namespace Notism.Shared.Configuration;

public class AwsSettings
{
    public const string SectionName = "AWS";

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
}

