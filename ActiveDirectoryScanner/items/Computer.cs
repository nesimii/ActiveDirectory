namespace ActiveDirectoryScanner.items
{
    public class Computer
    {
        public string objectId { get; set; }
        public string distinguishedName { get; set; }
        public string OperatingSystem { get; set; }
        public DateTime whenCreated { get; set; }
        public string securityDescriptor { get; set; }
        public List<string> groupObjectIds { get; set; }
    }
}
