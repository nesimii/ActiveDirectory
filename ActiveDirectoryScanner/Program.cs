using ActiveDirectoryScanner.activeDirectory.interfaces;
using ActiveDirectoryScanner.activeDirectory;
using ActiveDirectoryScanner.database;
using ActiveDirectoryScanner.items;
using ActiveDirectoryScanner.database.interfaces;

Console.WriteLine("Forestall AD Scanner\n");

//eski yapı
{
    IActiveDirectory activeDirectory = new ActiveDirectory("LDAP://nesimi.local", "Administrator", "admin_123");

    activeDirectory.getGroups();
    activeDirectory.getUsers();
    activeDirectory.getComputers();
}

//test kayıt yapısı
//{
//    IDatabase db = new MyNeo4jClient();

//    Group group1 = new Group();

//    group1.objectId = "123";
//    group1.distinguishedName = "nesimi";
//    db.saveGroup(group1);

//    User user1 = new User();
//    user1.objectId = "123";
//    user1.distinguishedName = "nesimi";
//    db.SaveUser(user1);

//    Computer computer1 = new Computer();
//    computer1.objectId = "123";
//    computer1.distinguishedName = "pc1";
//    db.saveComputer(computer1);
//}
Console.WriteLine("\nAd scanner is done...");

Console.WriteLine("press any key for close this window");
Console.ReadLine();
