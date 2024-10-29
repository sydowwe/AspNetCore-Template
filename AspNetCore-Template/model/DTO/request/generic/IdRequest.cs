using System.ComponentModel.DataAnnotations;
using AspNetCore_Template.model.DTO.request.extendable;

namespace AspNetCore_Template.model.DTO.request.generic;

public class IdRequest : IRequest
{
    [Required] public long Id { get; set; }
}