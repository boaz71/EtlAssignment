using System.Text.Json;
using System.Text;

// Path to the local events file
var filePath = "events.jsonl";

// Read all lines (each line is a separate JSON event)
if (!File.Exists(filePath))
{
    Console.WriteLine("events.jsonl file not found.");
    return;
}

var lines = File.ReadAllLines(filePath);

// HTTP client for sending requests
using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://localhost:5000"); // Update if your server runs on different port

// Add Authorization header for all requests
httpClient.DefaultRequestHeaders.Add("Authorization", "secret");


// Loop through each line and send it to the server
foreach (var line in lines)
{
    try
    {
        // Try to parse the JSON line
        var jsonDoc = JsonDocument.Parse(line);
        var content = new StringContent(line, Encoding.UTF8, "application/json");

        // Send POST request to /events/liveEvent
        var response = await httpClient.PostAsync("/events/liveEvent", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Sent event: {line}");
        }
        else
        {
            Console.WriteLine($"Failed to send event: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending event: {ex.Message}");
    }
}

Console.WriteLine("All events sent.");
