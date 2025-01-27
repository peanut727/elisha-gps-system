using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elisha_gps_reservation_system.Data
{
    public class IncidentLog
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; }
        
        public double PreviousSpeed { get; set; }
        public double CurrentSpeed { get; set; }
        public double TimeDiff { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        // Foreign key for Vehicle
        public int VehicleId { get; set; }
        
        [ForeignKey("VehicleId")]
        public Vehicle Vehicle { get; set; }
    }
}