namespace MongoDB_Web.Objects
{
    public class OTPFileObject
    {
        public Guid UUID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expire { get; set; }
        public string? RandomString { get; set; }
        public DateTime? LastAccess { get; set; }
        public string LastIpOfRequest { get; set; }
        public bool OnTokenDeleteMongodbUser { get; set; }
        public string Username { get; set; }

        public OTPFileObject(Guid uuid, DateTime created, string? randomstring, string ipOfRequest, bool onTokenDeleteMongodbUser, string username)
        {
            UUID = uuid;
            Created = created;
            RandomString = randomstring;
            LastAccess = DateTime.Now;
            LastIpOfRequest = ipOfRequest;
            OnTokenDeleteMongodbUser = onTokenDeleteMongodbUser;
            Username = username;
        }
    }
}
