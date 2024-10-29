using AspNetCore_Template.model.DTO.response.extendable;

namespace AspNetCore_Template.model.DTO.response.user;

public class UserResponse : IdResponse
{
    public string Email { get; set; }
    public bool TwoFactorEnabled { get; set; }
}