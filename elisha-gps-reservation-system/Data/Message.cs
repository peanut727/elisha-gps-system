using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gps_tracking_system.Data;

public class Message
{
    [Key]
    public int MessageId { get; set; }

    [Required]
    [ForeignKey("Sender")]
    public int SenderId { get; set; }

    [Required]
    [ForeignKey("Receiver")]
    public int ReceiverId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string MessageText { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User Sender { get; set; }
    public virtual User Receiver { get; set; }
}