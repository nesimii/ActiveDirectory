using ActiveDirectoryScanner.database.interfaces;
using ActiveDirectoryScanner.items;
using Neo4j.Driver;

namespace ActiveDirectoryScanner.database
{
    public class MyNeo4j : IDatabase
    {
        private static MyNeo4j myNeo4J;
        private readonly IDriver _driver;
        private readonly string uri = "bolt://localhost:7687";
        private readonly string user = "neo4j";
        private readonly string password = "password";

        private MyNeo4j()
        {
            _driver = GraphDatabase.Driver(this.uri, AuthTokens.Basic(this.user, this.password));
        }
        //signleton design
        public MyNeo4j GetMyNeo4J()
        {
            if (myNeo4J == null) myNeo4J = new MyNeo4j();
            return myNeo4J;
        }


        public async void SaveUser(User user, List<string> groupObjectSids)
        {
            using (var session = _driver.AsyncSession())
            {
                string query = @"
                CREATE (u:User 
                {
                    objectSid: $objectSid,
                    distinguishedName: $distinguishedName,
                    whenCreated: datetime($whenCreated),
                    pwdLastSet: datetime($pwdLastSet),
                    servicePrincipalName: $servicePrincipalName,
                    securityDescriptor: $securityDescriptor,
                    genericAll: $genericAll,
                    writeDacl: $writeDacl
                })";
                object parameters = new
                {
                    objectSid = user.objectSid,
                    distinguishedName = user.distinguishedName,
                    whenCreated = user.whenCreated.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                    pwdLastSet = user.pwdLastSet.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                    servicePrincipalName = user.servicePrincipalName,
                    securityDescriptor = user.securityDescriptor,
                    genericAll = user.genericAll,
                    writeDacl = user.writeDacl
                };

                if (groupObjectSids.Count > 0)
                {
                    string groupMatches = "MATCH";
                    string createRelations = "CREATE";
                    for (int i = 0; i < groupObjectSids.Count; i++)
                    {
                        groupMatches += $"(g{i}:Group {{objectSid: '{groupObjectSids[i]}'}}),";
                        createRelations += $"(u)-[:MEMBER_OF]->(g{i}),";
                    }
                    string newgroupMatches = groupMatches.Substring(0, groupMatches.Length - 1);
                    string newcreateRelations = createRelations.Substring(0, createRelations.Length - 1);
                    query = $"{query}\nWITH u\n{newgroupMatches}\n{newcreateRelations}";
                    Console.WriteLine(query);
                }
                try
                {
                    await session.WriteTransactionAsync(async tx => await tx.RunAsync(query, parameters));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    new Exception("kullanıcı kaydedilirken hata oluştu..");
                }
                finally { await session.CloseAsync(); }
            }
        }

        public async void saveComputer(Computer computer, List<string> groupObjectSids)
        {
            using (var session = _driver.AsyncSession())
            {
                var query = @"
            CREATE (c:Computer {
                objectSid: $objectSid,
                distinguishedName: $distinguishedName,
                operatingSystem: $operatingSystem,
                whenCreated: datetime($whenCreated),
                securityDescriptor: $securityDescriptor
            })";

                var parameters = new
                {
                    objectSid = computer.objectSid,
                    distinguishedName = computer.distinguishedName,
                    operatingSystem = computer.operatingSystem,
                    whenCreated = computer.whenCreated.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                    securityDescriptor = computer.securityDescriptor
                };

                if (groupObjectSids.Count > 0)
                {
                    string groupMatches = "MATCH";
                    string createRelations = "CREATE";
                    for (int i = 0; i < groupObjectSids.Count; i++)
                    {
                        groupMatches += $"(g{i}:Group {{objectSid: '{groupObjectSids[i]}'}}),";
                        createRelations += $"(c)-[:MEMBER_OF]->(g{i}),";
                    }
                    string newgroupMatches = groupMatches.Substring(0, groupMatches.Length - 1);
                    string newcreateRelations = createRelations.Substring(0, createRelations.Length - 1);
                    query = $"{query}\nWITH c\n{newgroupMatches}\n{newcreateRelations}";
                    Console.WriteLine(query);
                }
                try
                {
                    await session.WriteTransactionAsync(async tx => await tx.RunAsync(query, parameters));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    new Exception("bilgisayar kaydedilirken hata oluştu..");
                }
                finally { await session.CloseAsync(); }
            }
        }

        public async void saveGroup(Group group)
        {
            using (var session = _driver.AsyncSession())
            {
                var query = @"
            CREATE (g:Group {
                objectSid: $objectSid,
                distinguishedName: $distinguishedName,
                description: $description,
                whenCreated: datetime($whenCreated),
                securityDescriptor: $securityDescriptor,
                genericAll: $genericAll,
                writeDacl: $writeDacl
            })";

                var parameters = new
                {
                    objectSid = group.objectSid,
                    distinguishedName = group.distinguishedName,
                    description = group.description,
                    whenCreated = group.whenCreated.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                    securityDescriptor = group.securityDescriptor,
                    genericAll = group.genericAll,
                    writeDacl = group.writeDacl
                };

                try
                {
                    await session.WriteTransactionAsync(async tx => await tx.RunAsync(query, parameters));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    new Exception("grup kaydedilirken hata oluştu..");
                }
                finally { await session.CloseAsync(); }
            }
        }

        private async void setRelations(string object_Id, string type, List<string> list)
        {
            using (var session = _driver.AsyncSession())
            {
                try
                {
                    foreach (var group_Id in list)
                    {
                        var relationQuery = $"MATCH (i:{type} {{objectId: $objectId}}), (g:Group {{objectSid: $groupId}}) CREATE (i)-[:MEMBER_OF]->(g)";

                        var relationParameters = new
                        {
                            objectId = object_Id,
                            groupId = group_Id
                        };
                        await session.WriteTransactionAsync(async tx =>
                        {
                            await tx.RunAsync(relationQuery, relationParameters);
                            return true;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("relation oluşturulamadı");
                }
                finally { await session.CloseAsync(); }

            }
        }
    }
}
