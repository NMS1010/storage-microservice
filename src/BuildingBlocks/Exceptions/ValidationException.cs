namespace BuildingBlocks.Exceptions
{
    public class ValidationException(
        string message,
        int? code
    ) : CustomException(message, code: code)
    {
    }
}
