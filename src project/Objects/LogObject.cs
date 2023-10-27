namespace MongoDB_Web.Objects
{
    public class LogObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Created { get; set; }
        public string? Message { get; set; }
        public string? Type { get; set; }
    }
}
