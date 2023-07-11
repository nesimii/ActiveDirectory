using ActiveDirectoryScanner.database;
using ActiveDirectoryScanner.database.interfaces;
using ActiveDirectoryScanner.items;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ActiveDirectoryScanner.activeDirectory.abstracts
{
    public abstract class ActiveDirectoryBase
    {
        private string domainPath { get; set; } //LDAP domain adress
        private string username { get; set; } //user
        private string password { get; set; } //pass
        private DirectoryEntry entry;
        private IDatabase database;

        public ActiveDirectoryBase(string domainPath, string username, string password)
        {
            this.domainPath = domainPath;
            this.username = username;
            this.password = password;
            entry = new DirectoryEntry(domainPath, username, password);
            database = new MyNeo4jClient();
        }
        public void searchUsers()
        {
            // Kullanıcılar
            DirectorySearcher userSearcher = new DirectorySearcher(entry);
            userSearcher.Filter = "(objectCategory=user)";
            string[] columns = new string[] { "memberOf", "distinguishedName", "objectSid", "servicePrincipalName", "nTSecurityDescriptor", "whenCreated", "pwdlastset", };
            userSearcher.PropertiesToLoad.AddRange(columns);

            try
            {
                Console.WriteLine("Kullanıcılar:");
                SearchResultCollection usersResult = userSearcher.FindAll();
                foreach (SearchResult userResult in usersResult)
                {
                    string objectId = new SecurityIdentifier((byte[])userResult.Properties["objectSid"][0], 0).ToString();
                    string distinguishedName = userResult.Properties["distinguishedName"][0].ToString();

                    DateTime whenCreatedUtc = (DateTime)userResult.Properties["whenCreated"][0];
                    DateTime whenCreatedLocal = convertLocalTime(whenCreatedUtc);

                    string servicePrincipalName;
                    if (userResult.Properties.Contains("servicePrincipalName"))
                    {
                        servicePrincipalName = userResult.Properties["servicePrincipalName"][0].ToString();
                    }
                    else
                    {
                        servicePrincipalName = "not found";
                    }
                    long pwdLastSetTimestamp = (long)userResult.Properties["pwdlastset"][0];
                    DateTime pwdLastSetUtc = DateTime.FromFileTimeUtc(pwdLastSetTimestamp);
                    DateTime pwdLastSetLocal = convertLocalTime(pwdLastSetUtc);

                    byte[] ntSecurityDescriptorBytes = (byte[])userResult.Properties["nTSecurityDescriptor"][0];
                    RawSecurityDescriptor securityDescriptor = new RawSecurityDescriptor(ntSecurityDescriptorBytes, 0);
                    string securityDescriptorString = securityDescriptor.GetSddlForm(AccessControlSections.All);

                    bool hasGenericAll = CheckPermission(userResult.GetDirectoryEntry(), ActiveDirectoryRights.GenericAll);
                    bool hasWriteDacl = CheckPermission(userResult.GetDirectoryEntry(), ActiveDirectoryRights.WriteDacl);

                    User user = new User();
                    user.objectSid = objectId;
                    user.distinguishedName = distinguishedName;
                    user.whenCreated = whenCreatedUtc;
                    user.pwdLastSet = pwdLastSetUtc;
                    user.servicePrincipalName = servicePrincipalName;
                    user.securityDescriptor = securityDescriptorString;
                    user.genericAll = hasGenericAll;
                    user.writeDacl = hasWriteDacl;

                    Console.WriteLine("distinguishedName: " + distinguishedName);
                    Console.WriteLine("objectSid: " + objectId);
                    Console.WriteLine("whenCreated: " + whenCreatedUtc);
                    Console.WriteLine("pwdlastset: " + pwdLastSetUtc);
                    Console.WriteLine("pwdlastset: " + pwdLastSetLocal);
                    Console.WriteLine("servicePrincipalName: " + servicePrincipalName);
                    Console.WriteLine("nTSecurityDescriptor: " + securityDescriptorString);
                    Console.WriteLine("hasGenericAll: " + hasGenericAll);
                    Console.WriteLine("hasWriteDacl: " + hasWriteDacl);

                    List<string> groupObjectSids = new List<string>();
                    if (userResult.Properties.Contains("memberOf"))
                    {
                        foreach (string groupName in userResult.Properties["memberOf"])
                        {
                            // get group objectId
                            DirectorySearcher groupSearcher = new DirectorySearcher(entry);
                            groupSearcher.Filter = $"(distinguishedName={groupName})";
                            groupSearcher.PropertiesToLoad.Add("objectSid");

                            SearchResult groupResult = groupSearcher.FindOne();
                            if (groupResult != null)
                            {
                                string groupObjectSid = new SecurityIdentifier((byte[])groupResult.Properties["objectSid"][0], 0).ToString().Trim();
                                Console.WriteLine($"member of: {groupName}\t id:{groupObjectSid}");
                                if (!groupObjectSid.Equals(null)) groupObjectSids.Add(groupObjectSid);
                            }
                        }
                    }
                    Console.WriteLine("-----------------------------------------");
                    Console.WriteLine();
                    database.SaveUser(user, groupObjectSids);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("kullanıcı bilgileri alınamadı..");
            }
        }
        public void searchComputers()
        {
            // Bilgisayarlar
            DirectorySearcher computerSearcher = new DirectorySearcher(entry);
            computerSearcher.Filter = "(objectCategory=computer)";
            string[] columns = new string[] { "memberOf", "distinguishedName", "objectSid", "operatingSystem", "nTSecurityDescriptor", "whenCreated" };
            computerSearcher.PropertiesToLoad.AddRange(columns);
            SearchResultCollection computerResults = computerSearcher.FindAll();

            Console.WriteLine("Bilgisayarlar:");
            foreach (SearchResult computerResult in computerResults)
            {
                Console.WriteLine();

                string objectId = new SecurityIdentifier((byte[])computerResult.Properties["objectSid"][0], 0).ToString();
                string distinguishedName = computerResult.Properties["distinguishedName"][0].ToString();
                string OperatingSystem = computerResult.Properties["operatingSystem"][0].ToString();

                byte[] ntSecurityDescriptorBytes = (byte[])computerResult.Properties["nTSecurityDescriptor"][0];
                RawSecurityDescriptor securityDescriptor = new RawSecurityDescriptor(ntSecurityDescriptorBytes, 0);
                string securityDescriptorString = securityDescriptor.GetSddlForm(AccessControlSections.All);

                DateTime whenCreatedUtc = (DateTime)computerResult.Properties["whenCreated"][0];
                DateTime whenCreatedLocal = convertLocalTime(whenCreatedUtc);

                Computer computer = new Computer();
                computer.objectSid = objectId;
                computer.distinguishedName = distinguishedName;
                computer.OperatingSystem = OperatingSystem;
                computer.securityDescriptor = securityDescriptorString;
                computer.whenCreated = whenCreatedUtc;

                Console.WriteLine("distinguishedName: " + distinguishedName);
                Console.WriteLine("objectSid: " + objectId);
                Console.WriteLine("OperatingSystem: " + OperatingSystem);
                Console.WriteLine("whenCreated: " + whenCreatedUtc);
                Console.WriteLine("nTSecurityDescriptor: " + securityDescriptorString);

                List<string> groupObjectSids = new List<string>();
                if (computerResult.Properties.Contains("memberOf"))
                {
                    foreach (string groupName in computerResult.Properties["memberOf"])
                    {
                        // get group objectId
                        DirectorySearcher groupSearcher = new DirectorySearcher(entry);
                        groupSearcher.Filter = $"(distinguishedName={groupName})";
                        groupSearcher.PropertiesToLoad.Add("objectSid"); // assuming objectGUID is what you mean by objectId

                        SearchResult groupResult = groupSearcher.FindOne();
                        if (groupResult != null)
                        {
                            string groupObjectSid = new SecurityIdentifier((byte[])groupResult.Properties["objectSid"][0], 0).ToString().Trim();
                            Console.WriteLine($"member of: {groupName}\t id:{groupObjectSid}");
                            if (!groupObjectSid.Equals(null)) groupObjectSids.Add(groupObjectSid);
                        }
                    }
                }
                database.saveComputer(computer, groupObjectSids);
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine();
            }
        }
        public void searchGroups()
        {
            // Gruplar
            DirectorySearcher groupSearcher = new DirectorySearcher(entry);
            groupSearcher.Filter = "(objectCategory=group)";
            string[] columns = new string[] { "distinguishedName", "objectSid", "description", "nTSecurityDescriptor", "whenCreated" };
            groupSearcher.PropertiesToLoad.AddRange(columns);
            SearchResultCollection groupResults = groupSearcher.FindAll();

            Console.WriteLine("Gruplar:");
            foreach (SearchResult groupResult in groupResults)
            {
                string objectId = new SecurityIdentifier((byte[])groupResult.Properties["objectSid"][0], 0).ToString();
                string distinguishedName = groupResult.Properties["distinguishedName"][0].ToString();
                string description = groupResult.Properties["description"][0].ToString();

                byte[] ntSecurityDescriptorBytes = (byte[])groupResult.Properties["nTSecurityDescriptor"][0];
                RawSecurityDescriptor securityDescriptor = new RawSecurityDescriptor(ntSecurityDescriptorBytes, 0);
                string securityDescriptorString = securityDescriptor.GetSddlForm(AccessControlSections.All);

                DateTime whenCreatedUtc = (DateTime)groupResult.Properties["whenCreated"][0];
                DateTime whenCreatedLocal = convertLocalTime(whenCreatedUtc);

                bool hasGenericAll = CheckPermission(groupResult.GetDirectoryEntry(), ActiveDirectoryRights.GenericAll);
                bool hasWriteDacl = CheckPermission(groupResult.GetDirectoryEntry(), ActiveDirectoryRights.WriteDacl);

                Group group = new Group();
                group.objectSid = objectId;
                group.distinguishedName = distinguishedName;
                group.description = description;
                group.securityDescriptor = securityDescriptorString;
                group.whenCreated = whenCreatedUtc;
                group.genericAll = hasGenericAll;
                group.writeDacl = hasWriteDacl;
                database.saveGroup(group);

                Console.WriteLine("distinguishedName: " + distinguishedName);
                Console.WriteLine("objectSid: " + objectId);
                Console.WriteLine("description: " + description);
                Console.WriteLine("whenCreated: " + whenCreatedUtc);
                Console.WriteLine("nTSecurityDescriptor: " + securityDescriptorString);
                Console.WriteLine("hasGenericAll: " + hasGenericAll);
                Console.WriteLine("hasWriteDacl: " + hasWriteDacl);
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine();
            }
        }

        private DateTime convertLocalTime(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
        }
        private bool CheckPermission(DirectoryEntry entry, ActiveDirectoryRights right)
        {
            ActiveDirectorySecurity securityDescriptor = entry.ObjectSecurity;
            AuthorizationRuleCollection rules = securityDescriptor.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

            foreach (ActiveDirectoryAccessRule rule in rules)
            {
                if (rule.ActiveDirectoryRights.HasFlag(right))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
