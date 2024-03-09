using System.Net;

namespace BuildingBlocks.Exceptions
{
    public class CustomException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public int? Code { get; set; }

        public CustomException(
            string message,
            HttpStatusCode statusCode = HttpStatusCode.BadRequest,
            int? code = null) : base(message)
        {
            StatusCode = statusCode;
            Code = code;
        }
    }
}
