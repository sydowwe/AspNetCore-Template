namespace AspNetCore_Template.model.DTO.request.user;

public class ChangeEmailRequest : VerifyUserRequest
{
    public string NewEmail { get; set; }
}