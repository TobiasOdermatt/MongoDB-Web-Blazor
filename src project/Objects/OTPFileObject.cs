namespace MongoDB_Web.Objects
{
    public class OTPFileObject
    {
        public DateTime Created { get; set; }
        public string? RandomString { get; set; }
        public DateTime? LastAccess { get; set; }

        public OTPFileObject(DateTime created, string? randomstring)
        {
            Created = created;
            RandomString = randomstring;
            LastAccess = DateTime.Now;
        }
    }
}
