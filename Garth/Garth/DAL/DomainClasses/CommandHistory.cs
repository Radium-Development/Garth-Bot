using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Garth.DAL.DomainClasses;

public class CommandHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Module { get; set; }

    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Status { get; set; } // Success, Failed

    public string? ErrorReason { get; set; }
    
    public string? Error { get; set; }

    [Required]
    public ulong UserId { get; set; }
    
    [Required]
    public string User { get; set; }

    public string? Guild { get; set; }
    
    public ulong? GuildId { get; set; }
    
    [Required]
    public string Channel { get; set; }
    
    [Required]
    public ulong ChannelId { get; set; }
    
    [Required]
    public string FullCommand { get; set; }
    
    [Required]
    public ulong MessageId { get; set; }
    
    [Required]
    public DateTimeOffset Timestamp { get; set; }
    
    [Required]
    public string Environment { get; set; }
}