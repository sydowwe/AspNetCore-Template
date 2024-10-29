using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCore_Template.model.entity.abs;

public abstract class AbstractEntityWithUser : AbstractEntity
{
    protected AbstractEntityWithUser()
    {
    }

    protected AbstractEntityWithUser(long userId)
    {
        UserId = userId;
    }

    [Required] public long UserId { get; set; }

    [ForeignKey(nameof(UserId))] public virtual User User { get; set; }
}