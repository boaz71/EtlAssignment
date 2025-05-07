# ETL Project – Client / Server / Data Processor

This solution contains 4 small .NET 8 projects:

## 1. ServerApp
A Web API that listens on:

- `POST /events/liveEvent` – Receives events from the client and appends them to a local file (`server_events.log`)  
  > Requires `Authorization` header with value `"secret"`

- `GET /events/userEvents/{userId}` – Returns the current revenue for a given user (from the PostgreSQL DB)

- `GET /events/logfile` – Returns the full log file as plain text (JSONL format)

### Server address:
The server listens on:https://localhost:5000


## 2. ClientApp
A simple console app that reads a local file called `events.jsonl`,  
and sends each event (as a JSON line) to the server via `POST /events/liveEvent`  
with the required Authorization header.

## 3. DataProcessorApp
This app downloads the event log file from the server,  
processes the data line by line (add/subtract revenue),  
and updates the `users_revenue` table in PostgreSQL – once per user.

Revenue is calculated **in memory**, and then each user is updated in the database using an `UPSERT`.

## 4. SharedModels
A class library project that contains a shared model used by both ServerApp and DataProcessorApp:
EventModel
public class EventModel
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}
This model is used to represent events across the solution and keeps the structure consistent

## Database

Database table (db.sql)


create TABLE IF NOT EXISTS users_revenue (

create TABLE IF NOT EXISTS users_revenue (

    user_id VARCHAR(255) PRIMARY KEY,
    revenue INTEGER NOT NULL DEFAULT 0
);


## How to run the full flow

1.Make sure PostgreSQL is running and a database named etl_db exists

2.Run ServerApp (preferably from Visual Studio)

3.Run ClientApp – it will send events to the server

4.Run DataProcessorApp – it will process the events and update the DB


You can verify the result with a GET request to:
https://localhost:5000/events/userEvents/{userId}


## Notes

Everything is built with .NET 8

Events are stored in a simple text log file on the server

DataProcessor reads the file via HTTP, not local path

No Entity Framework – raw SQL is used

SharedModels avoids duplicated code between projects

Authorization is only required for /liveEvent endpoint

