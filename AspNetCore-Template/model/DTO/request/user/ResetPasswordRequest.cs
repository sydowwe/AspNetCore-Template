using AspNetCore_Template.model.DTO.request.extendable;

namespace AspNetCore_Template.model.DTO.request.user;

public class ResetPasswordRequest : IRequest
{
    public long UserId { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}