namespace BuildingBlocks.Exceptions
{
    public class ValidationException(
        string message
    ) : CustomException(message)
    {
    }
}
