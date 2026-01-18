namespace Notism.Shared.Exceptions;

public class ResultFailureException : Exception
{
    public string ErrorMessage { get; }

    public ResultFailureException(string errorMessage)
        : base(errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}