using ActiveDirectoryScanner.database.interfaces;
using ActiveDirectoryScanner.items;
using Neo4jClient;

namespace ActiveDirectoryScanner.database
{
    public class MyNeo4jClient : IDatabase
    {
        private static MyNeo4jClient myNeo4JClient;
        private readonly IGraphClient _graphClient;

        private MyNeo4jClient(string uri, string user, string password)
        {
            _graphClient = new BoltGraphClient(uri, user, password);
            _graphClient.ConnectAsync().Wait();
        }
        //singleton design
        public static MyNeo4jClient getmyNeo4JClient(string uri, string user, string password)
        {
            if (myNeo4JClient == null) myNeo4JClient = new MyNeo4jClient(uri, user, password);
            return myNeo4JClient;
        }
        public void saveComputer(Computer computer, List<string> groupObjectSids)
        {
            //objectId ile sorgular, kayıt yoksa kayıt eder.
            _graphClient.Cypher
                .Merge("(computer:Computer {objectSid: $id})")
                .OnCreate()
                .Set("computer = $newComputer")
                .WithParams(new
                {
                    id = computer.objectSid,
                    newComputer = computer
                })
                .ExecuteWithoutResultsAsync().Wait();

            //computer' ın bağlı olduğu gruplar varsa relation oluşturulur.
            if (groupObjectSids.Count > 0)
            {
                foreach (string groupObjectSid in groupObjectSids)
                {
                    _graphClient.Cypher
                        .Match("(c:Computer)", "(g:Group)")
                        .Where((Computer c) => c.objectSid == computer.objectSid)
                        .AndWhere((Group g) => g.objectSid == groupObjectSid)
                        .Merge("(c)-[:MEMBER_OF]->(g)")
                        .ExecuteWithoutResultsAsync().Wait();
                }
            }
        }

        public void saveGroup(Group group)
        {
            //objectId ile sorgular, kayıt yoksa kayıt eder.
            _graphClient.Cypher
                .Merge("(group:Group {objectSid: $id})")
                .OnCreate()
                .Set("group = $newGroup")
                .WithParams(new
                {
                    id = group.objectSid,
                    newGroup = group
                })
                .ExecuteWithoutResultsAsync().Wait();
        }

        public void SaveUser(User user, List<string> groupObjectSids)
        {
            //objectId ile sorgular, kayıt yoksa kayıt eder.
            _graphClient.Cypher
                .Merge("(user:User {objectSid: $id})")
                .OnCreate()
                .Set("user = $newUser")
                .WithParams(new
                {
                    id = user.objectSid,
                    newUser = user
                })
                .ExecuteWithoutResultsAsync().Wait();

            //user' ın bağlı olduğu gruplar varsa relation oluşturulur.
            if (groupObjectSids.Count > 0)
            {
                foreach (string groupObjectSid in groupObjectSids)
                {
                    _graphClient.Cypher
                        .Match("(u:User)", "(g:Group)")
                        .Where((User u) => u.objectSid == user.objectSid)
                        .AndWhere((Group g) => g.objectSid == groupObjectSid)
                        .Merge("(u)-[:MEMBER_OF]->(g)")
                        .ExecuteWithoutResultsAsync().Wait();
                }
            }
        }
    }
}
