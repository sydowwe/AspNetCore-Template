using AspNetCore_Template.model.entity;

namespace AspNetCore_Template.model.DTO.request.user;

public class UserRequest : EmailRequest
{
    public bool TwoFactorEnabled { get; set; }
    public string RecaptchaToken { get; set; }
    public AvailableLocales CurrentLocale { get; set; }
    public string Timezone { get; set; }
    public bool IsOAuth2Only { get; set; }
}