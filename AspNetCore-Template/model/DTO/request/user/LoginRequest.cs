using AspNetCore_Template.model.DTO.request.extendable;

namespace AspNetCore_Template.model.DTO.request.user;

public class LoginRequest : IRequest
{
    public bool StayLoggedIn { get; set; }
    public string RecaptchaToken { get; set; }
    public string Timezone { get; set; }
}