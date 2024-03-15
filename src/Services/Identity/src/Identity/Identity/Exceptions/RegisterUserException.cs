using BuildingBlocks.Exceptions;

namespace Identity.Identity.Exceptions
{
    public class RegisterUserException : AppException
    {
        public RegisterUserException(string message) : base(message)
        {
        }
    }
}
