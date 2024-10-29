using AspNetCore_Template.model.entity;

namespace AspNetCore_Template.model.DTO.request.user;

public class RegistrationRequest : UserRequest
{
    public string Password { get; set; }
    public string RecaptchaToken { get; set; }
    public AvailableLocales CurrentLocale { get; set; }
    public string Timezone { get; set; }
}