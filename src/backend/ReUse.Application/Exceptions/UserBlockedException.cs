using Microsoft.AspNetCore.Http;

using ReUse.Application.Errors;

namespace ReUse.Application.Exceptions;

public sealed class UserBlockedException : AppException
{
    public UserBlockedException()
        : base(
            message: "This account has been blocked by an administrator.",
            errorCode: ErrorsCode.UserBlocked,
            statusCode: StatusCodes.Status403Forbidden)
    {
    }
}