namespace ActiveDirectoryScanner.items
{
    public class Computer
    {
        public string objectSid { get; set; }
        public string distinguishedName { get; set; }
        public string operatingSystem { get; set; }
        public DateTime whenCreated { get; set; }
        public string securityDescriptor { get; set; }
    }
}
