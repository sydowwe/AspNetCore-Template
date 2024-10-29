using AspNetCore_Template.model.DTO.request.extendable;

namespace AspNetCore_Template.model.DTO.request.user;

public class ChangePasswordRequest : IRequest
{
    public string? TwoFactorAuthToken { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}