

using SharedModels;

namespace ServerApp.Services
{
    public class EventStorageService
    {
        private readonly string _filePath = "server_events.log";

        public void AppendEvent(EventModel ev)
        {
            var line = System.Text.Json.JsonSerializer.Serialize(ev);
            File.AppendAllText(_filePath, line + Environment.NewLine);
        }
    }
}
