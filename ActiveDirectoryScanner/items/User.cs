﻿namespace ActiveDirectoryScanner.items
{
    public class User
    {
        public string objectSid { get; set; }
        public string distinguishedName { get; set; }
        public DateTime whenCreated { get; set; }
        public string servicePrincipalName { get; set; }
        public DateTime pwdLastSet { get; set; }
        public string securityDescriptor { get; set; }
        public bool genericAll { get; set; }
        public bool writeDacl { get; set; }
    }
}
