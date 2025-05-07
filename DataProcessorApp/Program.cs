using System.Text.Json;
using Npgsql;
using System.Net.Http;
using SharedModels;

// Replace with your actual connection string
var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=etl_db";

// Replace with the actual URL of your server
var serverUrl = "https://localhost:5000/events/logfile";

// Create HTTP client
var httpClient = new HttpClient();

Console.WriteLine("Downloading event log from server...");

// Try to get the log file from the server
var response = await httpClient.GetAsync(serverUrl);

if (!response.IsSuccessStatusCode)
{
    Console.WriteLine("Failed to download event log file.");
    return;
}

var fileContent = await response.Content.ReadAsStringAsync();
var lines = fileContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

Console.WriteLine($"Read {lines.Length} lines from server_events.log");

// Prepare revenue calculation per user
var revenueMap = new Dictionary<string, int>();

foreach (var line in lines)
{
    try
    {
        var ev = JsonSerializer.Deserialize<EventModel>(line);
        if (ev == null) continue;

        if (!revenueMap.ContainsKey(ev.UserId))
            revenueMap[ev.UserId] = 0;

        if (ev.Name == "add_revenue")
            revenueMap[ev.UserId] += ev.Value;
        else if (ev.Name == "subtract_revenue")
            revenueMap[ev.UserId] -= ev.Value;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to parse line: " + ex.Message);
    }
}

// Connect to PostgreSQL and update users_revenue table
using var conn = new NpgsqlConnection(connectionString);
conn.Open();

foreach (var entry in revenueMap)
{
    var userId = entry.Key;
    var amount = entry.Value;

    var cmd = new NpgsqlCommand(@"
        INSERT INTO users_revenue (user_id, revenue)
        VALUES (@id, @amount)
        ON CONFLICT (user_id)
        DO UPDATE SET revenue = users_revenue.revenue + @amount;
    ", conn);

    cmd.Parameters.AddWithValue("id", userId);
    cmd.Parameters.AddWithValue("amount", amount);
    cmd.ExecuteNonQuery();

    Console.WriteLine($"Updated revenue for {userId}: {amount}");
}

Console.WriteLine("Revenue update completed.");

