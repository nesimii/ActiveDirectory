using ActiveDirectoryScanner.database.interfaces;
using ActiveDirectoryScanner.items;
using Neo4j.Driver;

namespace ActiveDirectoryScanner.database
{
    public class MyNeo4j : IDatabase
    {
        private readonly IDriver _driver;
        private readonly string uri = "bolt://localhost:7687";
        private readonly string user = "neo4j";
        private readonly string password = "password";

        public MyNeo4j()
        {
            _driver = GraphDatabase.Driver(this.uri, AuthTokens.Basic(this.user, this.password));
        }


        public async void SaveUser(User user, List<string> groupObjectId)
        {
            using (var session = _driver.AsyncSession())
            {
                string query = @"
                CREATE (u:user 
                {
                    objectId: $objectId,
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
                    objectId = user.objectId,
                    distinguishedName = user.distinguishedName,
                    whenCreated = user.whenCreated.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                    pwdLastSet = user.pwdLastSet.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                    servicePrincipalName = user.servicePrincipalName,
                    securityDescriptor = user.securityDescriptor,
                    genericAll = user.genericAll,
                    writeDacl = user.writeDacl
                };

                if (user.groupObjectIds.Count > 0)
                {
                    string groupMatches = "MATCH";
                    string createRelations = "CREATE";
                    for (int i = 0; i < user.groupObjectIds.Count; i++)
                    {
                        groupMatches += $"(g{i}:group {{objectId: '{user.groupObjectIds[i]}'}}),";
                        createRelations += $"(u)-[:BELONGS_TO]->(g{i}),";
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

        public async void saveComputer(Computer computer, List<string> groupObjectIds)
        {
            using (var session = _driver.AsyncSession())
            {
                var query = @"
            CREATE (c:computer {
                objectId: $objectId,
                distinguishedName: $distinguishedName,
                operatingSystem: $operatingSystem,
                whenCreated: datetime($whenCreated),
                securityDescriptor: $securityDescriptor
            })";

                var parameters = new
                {
                    objectId = computer.objectId,
                    distinguishedName = computer.distinguishedName,
                    operatingSystem = computer.OperatingSystem,
                    whenCreated = computer.whenCreated.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                    securityDescriptor = computer.securityDescriptor
                };

                if (computer.groupObjectIds.Count > 0)
                {
                    string groupMatches = "MATCH";
                    string createRelations = "CREATE";
                    for (int i = 0; i < computer.groupObjectIds.Count; i++)
                    {
                        groupMatches += $"(g{i}:group {{objectId: '{computer.groupObjectIds[i]}'}}),";
                        createRelations += $"(c)-[:BELONGS_TO]->(g{i}),";
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
            CREATE (g:group {
                objectId: $objectId,
                distinguishedName: $distinguishedName,
                description: $description,
                whenCreated: datetime($whenCreated),
                securityDescriptor: $securityDescriptor,
                genericAll: $genericAll,
                writeDacl: $writeDacl
            })";

                var parameters = new
                {
                    objectId = group.objectId,
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
                        var relationQuery = $"MATCH (i:{type} {{objectId: $objectId}}), (g:group {{objectId: $groupId}}) CREATE (i)-[:BELONGS_TO]->(g)";

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
