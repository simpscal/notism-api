namespace Notism.Shared.Configuration;

public class OutputCacheSettings
{
    public const string SectionName = "OutputCache";

    public bool Enabled { get; set; } = true;
    public int FoodsTtlSeconds { get; set; } = 60;
    public int CategoriesTtlSeconds { get; set; } = 300;
}
