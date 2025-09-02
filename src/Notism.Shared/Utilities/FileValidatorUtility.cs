using System.Text.RegularExpressions;

namespace Notism.Shared.Utilities;

public class FileValidatorUtility
{
    public static bool IsValidFileName(string fileName)
    {
        var pattern = @"^[a-zA-Z0-9_.-]+$";

        return Regex.IsMatch(fileName, pattern);
    }
}