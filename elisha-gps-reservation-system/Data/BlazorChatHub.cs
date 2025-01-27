using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace gps_tracking_system.Data;

public class BlazorChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public BlazorChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public const string HubUrl = "/chat";
    private const int MaxMessageLength = 1000;
    
    // Thread-safe dictionary for connected users
    private static readonly ConcurrentDictionary<string, UserConnection> UserConnections = new();

    private class UserConnection
    {
        public string ConnectionId { get; set; }
        public string Role { get; set; }
    }

    public async Task RegisterUser(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");
            }

            var connectionId = Context.ConnectionId;
            var role = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "User";
            
            var userConnection = new UserConnection 
            { 
                ConnectionId = connectionId,
                Role = role
            };

            if (!UserConnections.TryAdd(username, userConnection))
            {
                // Handle existing connection
                var existing = UserConnections[username];
                if (existing.ConnectionId != connectionId)
                {
                    await Clients.Client(existing.ConnectionId).SendAsync("ForceDisconnect", "New login detected");
                    UserConnections.TryUpdate(username, userConnection, existing);
                }
            }

            // Send the current user list to the newly connected user
            var connectedUsers = UserConnections.Keys.ToList();
            await Clients.Caller.SendAsync("ConnectedUsers", connectedUsers);
            
            // Notify others of the new connection
            await Clients.All.SendAsync("UserConnected", username);
            Console.WriteLine($"User {username} registered successfully with role {role}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering user {username}: {ex.Message}");
            throw;
        }
    }

    [Authorize]
    public async Task SendMessage(string sender, string receiver, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message) || message.Length > MaxMessageLength)
            {
                throw new ArgumentException("Invalid message content");
            }

            var senderUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == sender);
            var receiverUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == receiver);

            if (senderUser == null || receiverUser == null)
            {
                await Clients.Caller.SendAsync("MessageError", "User not found");
                return;
            }

            // Save message to database
            var dbMessage = new Message
            {
                SenderId = senderUser.UserId,
                ReceiverId = receiverUser.UserId,
                Sender = senderUser,
                Receiver = receiverUser,
                MessageText = message,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(dbMessage);
            await _context.SaveChangesAsync();

            // If receiver is connected, send to their connection
            if (UserConnections.TryGetValue(receiver, out var receiverConnection))
            {
                await Clients.Client(receiverConnection.ConnectionId)
                    .SendAsync("ReceiveMessage", sender, receiver, message);
            }

            // Send back to the sender
            await Clients.Caller.SendAsync("ReceiveMessage", sender, receiver, message);

            // If either user is admin, send to all other admin connections
            if (senderUser.Role == "Admin" || receiverUser.Role == "Admin")
            {
                var adminConnections = UserConnections
                    .Where(x => x.Value.Role == "Admin" && x.Key != sender)
                    .Select(x => x.Value.ConnectionId);

                foreach (var adminConnectionId in adminConnections)
                {
                    await Clients.Client(adminConnectionId)
                        .SendAsync("ReceiveMessage", sender, receiver, message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message from {sender} to {receiver}: {ex.Message}");
            await Clients.Caller.SendAsync("MessageError", "Failed to send message");
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = UserConnections.FirstOrDefault(x => x.Value.ConnectionId == Context.ConnectionId).Key;
        if (username != null)
        {
            UserConnections.TryRemove(username, out _);
            await Clients.All.SendAsync("UserDisconnected", username);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
