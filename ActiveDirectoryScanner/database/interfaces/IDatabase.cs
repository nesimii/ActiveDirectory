using ActiveDirectoryScanner.items;

namespace ActiveDirectoryScanner.database.interfaces
{
    internal interface IDatabase
    {
        void SaveUser(User user, List<string> groupObjectSids);
        void saveComputer(Computer computer, List<string> groupObjectSids);
        void saveGroup(Group group);
    }
}
