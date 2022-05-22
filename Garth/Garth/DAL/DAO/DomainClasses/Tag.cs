using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Garth.DAL.DAO.DomainClasses;

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
    public DateTime? CreationDate { get; set; }

    [Required]
    [DefaultValue(false)]
    public bool IsFile { get; set; }

    [Required]
    [DefaultValue("")]
    [StringLength(255)]
    public string? FileName { get; set; }

    [Required]
    [DefaultValue(false)]
    public bool Global { get; set; }

    [Required]
    public ulong Server { get; set; }
}