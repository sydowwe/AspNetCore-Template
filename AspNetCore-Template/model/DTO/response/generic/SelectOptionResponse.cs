using AspNetCore_Template.model.DTO.response.extendable;

namespace AspNetCore_Template.model.DTO.response.generic;

public class SelectOptionResponse : IdResponse
{
    public string Label { get; init; }
}