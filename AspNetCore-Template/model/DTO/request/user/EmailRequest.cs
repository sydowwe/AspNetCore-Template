using AspNetCore_Template.model.DTO.request.extendable;

namespace AspNetCore_Template.model.DTO.request.user;

public class EmailRequest : IRequest
{
    public string Email { get; set; }
}