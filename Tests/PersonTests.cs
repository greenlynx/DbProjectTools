using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    [Collection("Database tests")]
    public class PersonTests
    {
        private DatabaseFixture _database;

        public PersonTests(DatabaseFixture database) => _database = database;

        [Fact]
        public void Test1() => _database.ExecuteSql("insert into dbo.person (name, age, height) values ('Dan', 36, 1);");
    }
}
