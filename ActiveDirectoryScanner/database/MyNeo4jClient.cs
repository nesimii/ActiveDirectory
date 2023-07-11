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
        public async void saveComputer(Computer computer, List<string> groupIds)
        {
            //objectId ile sorgular, kayıt yoksa kayıt eder.
            await _graphClient.Cypher
                .Merge("(computer:Computer {objectId: $id})")
                .OnCreate()
                .Set("computer = $newComputer")
                .WithParams(new
                {
                    id = computer.objectId,
                    newComputer = computer
                })
                .ExecuteWithoutResultsAsync();
        }

        public async void saveGroup(Group group)
        {
            //objectId ile sorgular, kayıt yoksa kayıt eder.
            await _graphClient.Cypher
                .Merge("(group:Group {objectId: $id})")
                .OnCreate()
                .Set("group = $newGroup")
                .WithParams(new
                {
                    id = group.objectId,
                    newGroup = group
                })
                .ExecuteWithoutResultsAsync();
        }

        public async void SaveUser(User newUser, List<string> groupIds)
        {
            //objectId ile sorgular, kayıt yoksa kayıt eder.
            await _graphClient.Cypher
                .Merge("(user:User {objectId: $id})")
                .OnCreate()
                .Set("user = $newUser")
                .WithParams(new
                {
                    id = newUser.objectId,
                    newUser
                })
                .ExecuteWithoutResultsAsync();
        }
    }
}
