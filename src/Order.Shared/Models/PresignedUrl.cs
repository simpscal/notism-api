namespace Order.Shared.Models;

public class PresignedUrl
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
}