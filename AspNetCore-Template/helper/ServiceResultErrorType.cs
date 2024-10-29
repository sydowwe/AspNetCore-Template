namespace AspNetCore_Template.helper;

public enum ServiceResultErrorType
{
    AuthenticationFailed,
    UserLockedOut,
    EmailNotConfirmed,
    IdentityError,
    NotFound,
    BadRequest,
    Conflict,
    InternalServerError,
    TwoFactorAuthRequired,
    InvalidTwoFactorAuthToken
}