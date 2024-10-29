using AspNetCore_Template.model.DTO.response.extendable;

namespace AspNetCore_Template.model.DTO.response.user;

public class TwoFactorAuthResponse : IResponse
{
    public bool TwoFactorEnabled { get; set; }
    public string? QrCode { get; set; }
    public IEnumerable<string>? RecoveryCodes { get; set; }
}