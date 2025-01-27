public class Vehicle
{
    public int id { get; set; }
    public string? name { get; set; }
    public string? type { get; set; }
    public string? transmission { get; set; }
    public int? mileage { get; set; }
    public decimal price_per_day { get; set; }
    public string? capacity { get; set; }
    public string? description { get; set; }
    public string? imgsrc { get; set; }
    public required string status { get; set; }
    public required string gpsID { get; set; }
}
