using AspNetCore_Template.model.entity;

namespace AspNetCore_Template.model.DTO.response.user;

public class GoogleSignInResponse : EmailResponse
{
    public AvailableLocales CurrentLocale { get; set; }
}