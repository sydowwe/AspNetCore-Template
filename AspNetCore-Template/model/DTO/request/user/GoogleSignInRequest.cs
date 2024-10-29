namespace AspNetCore_Template.model.DTO.request.user;

public class GoogleSignInRequest : LoginRequest
{
    public string Code { get; set; }
}