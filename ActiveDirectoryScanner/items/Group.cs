namespace ActiveDirectoryScanner.items
{
    public class Group
    {
        public string objectSid { get; set; }
        public string distinguishedName { get; set; }
        public string description { get; set; }
        public DateTime whenCreated { get; set; }
        public string securityDescriptor { get; set; }
        public bool genericAll { get; set; }
        public bool writeDacl { get; set; }
    }
}
