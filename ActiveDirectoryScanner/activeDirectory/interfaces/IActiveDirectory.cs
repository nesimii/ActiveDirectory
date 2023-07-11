using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveDirectoryScanner.activeDirectory.interfaces
{
    public interface IActiveDirectory
    {
        void getUsers();
        void getComputers();
        void getGroups();
    }
}
