using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class VehicleReview
{
    [Key]
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public int VehicleId { get; set; }
    public int Rating { get; set; } // Value between 1 and 5
    public string? Comment { get; set; }
    public DateTime ReviewDate { get; set; } = DateTime.Now; // Default to current timestamp
}