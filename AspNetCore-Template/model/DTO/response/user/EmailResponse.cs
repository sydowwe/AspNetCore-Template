using AspNetCore_Template.model.DTO.response.extendable;

namespace AspNetCore_Template.model.DTO.response.user;

public class EmailResponse : IResponse
{
    public string Email { get; set; }
}