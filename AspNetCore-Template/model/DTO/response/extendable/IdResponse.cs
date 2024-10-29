namespace AspNetCore_Template.model.DTO.response.extendable;

public interface IIdResponse : IResponse
{
    public long Id { get; set; }
}

public class IdResponse : IIdResponse
{
    public IdResponse()
    {
    }

    public IdResponse(long id)
    {
        Id = id;
    }

    public long Id { get; set; }
}