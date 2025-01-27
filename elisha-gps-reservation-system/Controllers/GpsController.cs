using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace elisha_gps_reservation_system.Data;

[Route("api/[controller]")]
[ApiController]
public class GpsController : ControllerBase
{
    private static Dictionary<int, GpsData> _lastGpsDataByVehicle = new();
    private static Dictionary<int, int> _incidentCountByVehicle = new();
    private static Dictionary<int, bool> _isOverspeedingByVehicle = new();
    private static Dictionary<int, DateTime> _lastIncidentTimeByVehicle = new();
    private const double MIN_INCIDENT_INTERVAL = 0.5; // Minimum seconds between incidents
    private const double MAX_DETECTION_INTERVAL = 2.5; // Maximum seconds for incident detection
    private readonly IHubContext<GpsHub> _hubContext;

    public GpsController(IHubContext<GpsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> PostGpsData([FromBody] GpsData gpsData)
    {
        if (gpsData == null) return BadRequest("Invalid GPS DATA");
        
        // Get previous data for this specific vehicle
        _lastGpsDataByVehicle.TryGetValue(gpsData.VehicleId, out var previousGpsData);
        _lastGpsDataByVehicle[gpsData.VehicleId] = gpsData;

        // Initialize counters for new vehicles
        if (!_incidentCountByVehicle.ContainsKey(gpsData.VehicleId))
        {
            _incidentCountByVehicle[gpsData.VehicleId] = 0;
            _isOverspeedingByVehicle[gpsData.VehicleId] = false;
            _lastIncidentTimeByVehicle[gpsData.VehicleId] = DateTime.MinValue;
        }

        // Send update to clients with vehicle ID
        await _hubContext.Clients.All.SendAsync("ReceiveGpsUpdate", gpsData);
        
        if (previousGpsData != null)
        {
            var timeDiff = (gpsData.Timestamp - previousGpsData.Timestamp).TotalSeconds;
            var speedDiff = gpsData.Speed - previousGpsData.Speed; // Positive for acceleration
            
            Console.WriteLine($"Speed change: {previousGpsData.Speed:F1} -> {gpsData.Speed:F1} = {speedDiff:F1} km/h in {timeDiff:F1} seconds");
            
            var currentTime = DateTime.Now;
            var lastIncidentTime = _lastIncidentTimeByVehicle[gpsData.VehicleId];
            var timeSinceLastIncident = (currentTime - lastIncidentTime).TotalSeconds;
            
            // Only process incidents if enough time has passed and the speed change is significant
            if (timeSinceLastIncident >= MIN_INCIDENT_INTERVAL && (Math.Abs(speedDiff) >= 1.0 || gpsData.Speed > 100))
            {
                // Detect overspeeding (over 100 km/h)
                if (gpsData.Speed > 100)
                {
                    if (!_isOverspeedingByVehicle[gpsData.VehicleId])
                    {
                        _isOverspeedingByVehicle[gpsData.VehicleId] = true;
                        _incidentCountByVehicle[gpsData.VehicleId]++;
                        Console.WriteLine($"OVERSPEEDING DETECTED! Speed: {gpsData.Speed:F1} km/h, Count: {_incidentCountByVehicle[gpsData.VehicleId]}");
                        
                        await _hubContext.Clients.All.SendAsync("IncidentDetected", new
                        {
                            VehicleId = gpsData.VehicleId,
                            Type = "Overspeeding",
                            Speed = gpsData.Speed,
                            PreviousSpeed = previousGpsData.Speed,
                            CurrentSpeed = gpsData.Speed,
                            TimeDiff = timeDiff,
                            Location = new { gpsData.Latitude, gpsData.Longitude },
                            Count = _incidentCountByVehicle[gpsData.VehicleId],
                            Timestamp = gpsData.Timestamp
                        });
                        _lastIncidentTimeByVehicle[gpsData.VehicleId] = currentTime;
                    }
                }
                else
                {
                    _isOverspeedingByVehicle[gpsData.VehicleId] = false;
                }

                // Detect sudden braking (45 km/h decrease within 2.5s or 50% speed reduction)
                if (timeDiff <= MAX_DETECTION_INTERVAL && speedDiff < 0 && 
                    (Math.Abs(speedDiff) >= 45 || (previousGpsData.Speed > 20 && gpsData.Speed <= previousGpsData.Speed * 0.5)))
                {
                    _incidentCountByVehicle[gpsData.VehicleId]++;
                    Console.WriteLine($"SUDDEN BRAKING: Speed change {previousGpsData.Speed:F1} -> {gpsData.Speed:F1} km/h in {timeDiff:F1}s");
                    
                    await _hubContext.Clients.All.SendAsync("IncidentDetected", new
                    {
                        VehicleId = gpsData.VehicleId,
                        Type = "Braking",
                        PreviousSpeed = previousGpsData.Speed,
                        CurrentSpeed = gpsData.Speed,
                        TimeDiff = timeDiff,
                        Location = new { gpsData.Latitude, gpsData.Longitude },
                        Count = _incidentCountByVehicle[gpsData.VehicleId],
                        Timestamp = gpsData.Timestamp
                    });
                    _lastIncidentTimeByVehicle[gpsData.VehicleId] = currentTime;
                }
                
                // Detect sudden acceleration (30 km/h increase within 2.5s or 100% speed increase)
                if (timeDiff <= MAX_DETECTION_INTERVAL && speedDiff > 0 && 
                    (speedDiff >= 30 || (previousGpsData.Speed > 10 && gpsData.Speed >= previousGpsData.Speed * 2)))
                {
                    _incidentCountByVehicle[gpsData.VehicleId]++;
                    Console.WriteLine($"SUDDEN ACCELERATION: Speed change {previousGpsData.Speed:F1} -> {gpsData.Speed:F1} km/h in {timeDiff:F1}s");
                    
                    await _hubContext.Clients.All.SendAsync("IncidentDetected", new
                    {
                        VehicleId = gpsData.VehicleId,
                        Type = "Acceleration",
                        PreviousSpeed = previousGpsData.Speed,
                        CurrentSpeed = gpsData.Speed,
                        TimeDiff = timeDiff,
                        Location = new { gpsData.Latitude, gpsData.Longitude },
                        Count = _incidentCountByVehicle[gpsData.VehicleId],
                        Timestamp = gpsData.Timestamp
                    });
                    _lastIncidentTimeByVehicle[gpsData.VehicleId] = currentTime;
                }
            }
        }

        return Ok();
    }

    [HttpGet]
    public IActionResult GetLastGpsData()
    {
        if (_lastGpsDataByVehicle.Count == 0)
            return NotFound("No GPS data available");

        return Ok(new { 
            GpsData = _lastGpsDataByVehicle.Values.ToList(), 
            IncidentCount = _incidentCountByVehicle.Values.ToList()
        });
    }
}

public record GpsData
{
    public DateTime Timestamp { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double Speed { get; init; }
    public int VehicleId { get; init; } = 21;
}