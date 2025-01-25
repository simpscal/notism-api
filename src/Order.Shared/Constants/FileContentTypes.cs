using System.Reflection;

namespace Order.Shared.Constants;

public static class FileContentTypes
{
    public const string ImageJpeg = "image/jpeg";
    public const string ImageJpg = "image/jpg";
    public const string ImagePng = "image/png";
    public const string ImageGif = "image/gif";
    public const string ImageBmp = "image/bmp";
    public const string ImageSvgXml = "image/svg+xml";

    public const string VideoMp4 = "video/mp4";
    public const string VideoWebm = "video/webm";
    public const string AudioMpeg = "audio/mpeg";
    public const string AudioOgg = "audio/ogg";

    public static readonly IEnumerable<string> All = typeof(FileContentTypes)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(field => field is { IsLiteral: true, IsInitOnly: false })
        .Select(field => field.GetValue(null)?.ToString()!);
}