namespace AspNetCore_Template.model.DTO.request.user;

public class TwoFactorAuthLoginRequest : TwoFactorAuthRequest
{
    public bool StayLoggedIn { get; set; }
    // public bool RememberClient { get; set; }
}