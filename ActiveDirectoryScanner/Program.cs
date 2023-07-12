using ActiveDirectoryScanner.activeDirectory.interfaces;
using ActiveDirectoryScanner.activeDirectory;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using ActiveDirectoryScanner.database;
using ActiveDirectoryScanner.items;
using ActiveDirectoryScanner.database.interfaces;

Console.WriteLine("Forestall AD Scanner\n");

{
    string uri = "LDAP://" + GetValue("LDAP uri: ");
    string user = GetValue("username: ");
    string pass = GetValue("password: ");
    ;
    try
    {
        //LDAP uri,user,pass ve kaydedilecek db belirtiliyor.
        IActiveDirectory activeDirectory = new ActiveDirectory(uri, user, pass, MyNeo4jClient.getmyNeo4JClient());

        //İlk gruplar taranıyor. Gruplar kaydedilince, userlar ve computerlar kaydedilirken relation kuruluyor.
        activeDirectory.getGroups();
        activeDirectory.getUsers();
        activeDirectory.getComputers();
    }
    catch (DirectoryServicesCOMException) { Console.WriteLine("kullanıcı adı veya parola hatalı.."); }
    catch (COMException) { Console.WriteLine("sunucu bulunamadı.."); }
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

string GetValue(string message)
{
    string input;
    do { Console.Write(message); input = Console.ReadLine(); } while (string.IsNullOrWhiteSpace(input));
    return input;
}