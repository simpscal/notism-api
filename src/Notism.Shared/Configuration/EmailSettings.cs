namespace Notism.Shared.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Notism";
}

