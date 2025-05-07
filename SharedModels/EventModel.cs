namespace SharedModels
{
    public class EventModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; // "add_revenue" or "subtract_revenue"
        public int Value { get; set; }
    }
}
