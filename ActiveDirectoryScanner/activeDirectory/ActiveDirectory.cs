using ActiveDirectoryScanner.activeDirectory.abstracts;
using ActiveDirectoryScanner.activeDirectory.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveDirectoryScanner.activeDirectory
{
    public class ActiveDirectory : ActiveDirectoryBase, IActiveDirectory
    {
        public ActiveDirectory(string domainPath, string username, string password) : base(domainPath, username, password)
        {
        }

        public void getUsers() => searchUsers();
        public void getComputers() => searchComputers();
        public void getGroups() => searchGroups();


    }
}
