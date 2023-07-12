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
        public ActiveDirectory(string domainPath, string username, string password, Object db)
            : base(domainPath, username, password, db)
        {
        }
        //abstract da tanımlı fonk istek atıyor
        public void getUsers() => searchUsers();
        public void getComputers() => searchComputers();
        public void getGroups() => searchGroups();


    }
}
