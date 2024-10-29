using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCore_Template.model.entity.abs;

public abstract class
    AbstractEntity
{
    protected AbstractEntity()
    {
        CreatedTimestamp = DateTime.UtcNow;
        ModifiedTimestamp = DateTime.UtcNow;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required] public DateTime CreatedTimestamp { get; set; }

    [Required] public DateTime ModifiedTimestamp { get; set; }
}