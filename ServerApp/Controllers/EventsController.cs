using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ServerApp.Models;
using ServerApp.Services;
using SharedModels;

namespace ServerApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventStorageService _storage;
        private readonly IConfiguration _configuration;

        public EventsController(EventStorageService storage, IConfiguration configuration)
        {
            _storage = storage;
            _configuration = configuration;
        }

        // POST /events/liveEvent
        [HttpPost("liveEvent")]
        public IActionResult ReceiveEvent([FromBody] EventModel ev)
        {
            // Check for Authorization header
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != "secret")
            {
                // If secret is wrong or missing, reject the request
                return Unauthorized("Missing or invalid Authorization header");
            }

            // Save event to a local file
            _storage.AppendEvent(ev);

            return Ok(new { message = "Event received" });
        }


        // GET /events/userEvents/{userId}
        [HttpGet("userEvents/{userId}")]
        public IActionResult GetUserRevenue(string userId)
        {
            // Connect to PostgreSQL
            var connStr = _configuration.GetConnectionString("Postgres");
            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            // Prepare query to get user revenue
            var cmd = new NpgsqlCommand("SELECT user_id, revenue FROM users_revenue WHERE user_id = @id", conn);
            cmd.Parameters.AddWithValue("id", userId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return NotFound(new { message = "User not found in database" });
            }

            // Read the result and return as JSON
            var result = new UserRevenue
            {
                UserId = reader.GetString(0),
                Revenue = reader.GetInt32(1)
            };

            return Ok(result);
        }

        // GET /events/logfile
        [HttpGet("logfile")]
        public IActionResult GetLogFile()
        {
            var filePath = "server_events.log";
            if (!System.IO.File.Exists(filePath))
                return NotFound("Event log file not found");

            var content = System.IO.File.ReadAllText(filePath);
            return Content(content, "text/plain");
        }

        // GET /events/userEvents/{userId}
        [HttpGet("helloWorld")]
        public IActionResult HelloWorld()
        {
   

            return Ok("Hello World!");
        }


    }
}
