using AspNetCore_Template.model.DTO.request.extendable;

namespace AspNetCore_Template.model.DTO.request.user;

public class TwoFactorAuthRequest : IRequest
{
    public string Token { get; set; }
}