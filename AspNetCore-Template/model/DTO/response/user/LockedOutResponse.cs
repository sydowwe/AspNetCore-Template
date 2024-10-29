namespace AspNetCore_Template.model.DTO.response.user;

public class LockedOutResponse
{
    public string Status { get; } = "lockedOut";
    public int Seconds { get; set; }
}