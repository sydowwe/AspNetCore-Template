namespace AspNetCore_Template.model.DTO.request.user;

public class PasswordLoginRequest : LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}