using System.Net;

using ReUse.Application.Errors;

namespace ReUse.Application.Exceptions;

public class UserLockedOutException : AppException
{
    public UserLockedOutException()
        : base(
            message: "User account is temporarily locked due to multiple failed login attempts.",
            errorCode: ErrorsCode.UserLockedOut,
            statusCode: (int)HttpStatusCode.Forbidden)
    {
    }
}