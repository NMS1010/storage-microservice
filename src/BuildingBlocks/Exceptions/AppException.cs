namespace BuildingBlocks.Exceptions
{
    public class AppException(
       string message,
       int? code = null
    ) : CustomException(message, code: code)
    {
    }
}
