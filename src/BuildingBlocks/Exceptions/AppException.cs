using Microsoft.AspNetCore.Http;
using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class AppException(
       string message
    ) : CustomException(message, (HttpStatusCode)StatusCodes.Status500InternalServerError)
    {
    }
}
