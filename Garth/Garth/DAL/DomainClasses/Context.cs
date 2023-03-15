using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Garth.DAL.DomainClasses;

public class Context
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public ulong CreatorId { get; set; }

    public bool Enabled { get; set; } = true;

    public string Value { get; set; }
}