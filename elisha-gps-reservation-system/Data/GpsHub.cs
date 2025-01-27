using Microsoft.AspNetCore.SignalR;

namespace elisha_gps_reservation_system.Data;

public class GpsHub : Hub
{
    public async Task SendGpsUpdate(GpsData gpsData)
    {
        await Clients.All.SendAsync("ReceiveGpsUpdate", gpsData);
    }
}