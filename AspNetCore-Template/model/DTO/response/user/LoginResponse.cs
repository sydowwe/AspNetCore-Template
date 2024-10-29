using AspNetCore_Template.model.entity;

namespace AspNetCore_Template.model.DTO.response.user;

public class LoginResponse : EmailResponse
{
    public bool RequiresTwoFactor { get; set; }
    public AvailableLocales CurrentLocale { get; set; }
}