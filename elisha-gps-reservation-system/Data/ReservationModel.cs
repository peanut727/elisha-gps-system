using System.ComponentModel.DataAnnotations;

public class ReservationModel
{
    [Key]
    public int ReservationId { get; set; }

    // Required fields with no default values, ensuring they are set upon initialization
    public required int UserId { get; set; }
    public required int VehicleId { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }

    // Optional fields with default values or nullable types for flexibility
    public decimal TotalAmount { get; set; } = 0.0m; // Default value for TotalAmount
    public required string Status { get; set; } = "Pending"; // Default status is "Pending"
    public string? VehicleBefore { get; set; } // Nullable for optional field
    public string? VehicleAfter { get; set; }  // Nullable for optional field
    public required string PickUpOrDropOff { get; set; }  // Required field for PickUpOrDropOff

    // Optional fields
    public string? Location { get; set; }       // Nullable for optional Location field
    public string? gpsID { get; set; }          // Nullable for optional GPS ID field
}