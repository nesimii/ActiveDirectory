using ActiveDirectoryScanner.items;

namespace ActiveDirectoryScanner.database.interfaces
{
    internal interface IDatabase
    {
        void SaveUser(User user, List<string> groupObjectIds);
        void saveComputer(Computer computer, List<string> groupObjectIds);
        void saveGroup(Group group);
    }
}
