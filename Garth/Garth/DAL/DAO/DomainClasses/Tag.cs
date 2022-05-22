using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Garth.DAL.DAO.DomainClasses;

[Index(nameof(Name), IsUnique = true)]
public class Tag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string? Name { get; set; }

    [Required]
    [Column(TypeName = "LONGTEXT")]
    public string? Content { get; set; }

    [Required]
    [StringLength(37)]
    public string? CreatorName { get; set; }

    [Required]
    public ulong CreatorId { get; set; }

    [Required]
    public DateTime? CreationDate { get; set; } = DateTime.Now;

    [Required]
    [DefaultValue(false)]
    public bool IsFile { get; set; } = false;

    [Required]
    [DefaultValue("")]
    [StringLength(255)]
    public string? FileName { get; set; } = string.Empty;

    [Required]
    [DefaultValue(false)]
    public bool Global { get; set; } = false;

    [Required]
    public ulong Server { get; set; }
}