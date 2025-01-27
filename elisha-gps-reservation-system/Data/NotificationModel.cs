using System.ComponentModel.DataAnnotations;

public class NotificationModel
{
    [Key]
    public int NotificationID { get; set; }
    public int? UserID { get; set; } // Nullable to account for potential null values
    public int? ReservationID { get; set; } // Nullable for optional relationship
    public int? MessageID { get; set; } // Nullable for optional relationship
    public int? ReviewID { get; set; } // Nullable for optional relationship
    public string NotifType { get; set; } = string.Empty; // ENUM equivalent as string
    public string Title { get; set; } = string.Empty; // Required, default to empty string
    public string Content { get; set; } = string.Empty; // Required, default to empty string
    public bool IsRead { get; set; } = false; // Defaults to false
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current timestamp
}
