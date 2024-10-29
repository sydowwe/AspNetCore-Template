using AspNetCore_Template.model.DTO.request.extendable;

namespace AspNetCore_Template.model.DTO.request.user;

public class VerifyUserRequest : IRequest
{
    public string? TwoFactorAuthToken { get; set; }
    public string Password { get; set; }
}