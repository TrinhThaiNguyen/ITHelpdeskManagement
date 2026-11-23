
using Microsoft.AspNetCore.SignalR.Client;

Console.Write("Enter JWT token: ");
var token = Console.ReadLine();

if (string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("JWT token is required.");
    return;
}

var connection = new HubConnectionBuilder()
    .WithUrl(
        "http://localhost:5008/hubs/notifications",
        options =>
        {
            options.AccessTokenProvider = () =>
                Task.FromResult<string?>(token);
        })
    .WithAutomaticReconnect()
    .Build();

// Lắng nghe sự kiện từ Server
connection.On<object>(
    "NotificationReceived",
    notification =>
    {
        Console.WriteLine();
        Console.WriteLine("=== NEW NOTIFICATION ===");
        Console.WriteLine(notification);
        Console.WriteLine("========================");
    });

try
{
    await connection.StartAsync();

    Console.WriteLine("Connected to Notification Hub!");
    Console.WriteLine("Waiting for notifications...");
    Console.WriteLine("Press Enter to exit.");

    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
}
finally
{
    await connection.DisposeAsync();
}
