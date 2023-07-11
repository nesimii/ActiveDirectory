using ActiveDirectoryScanner.database.interfaces;
using ActiveDirectoryScanner.items;
using Neo4jClient;

namespace ActiveDirectoryScanner.database
{
    public class MyNeo4jClient : IDatabase
    {
        private readonly IGraphClient _graphClient;
        private readonly string uri = "bolt://localhost:7687";
        private readonly string user = "neo4j";
        private readonly string password = "password";

        public MyNeo4jClient()
        {
            _graphClient = new BoltGraphClient(this.uri, this.user, this.password);
            _graphClient.ConnectAsync().Wait();
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
