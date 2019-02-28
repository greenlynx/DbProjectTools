using DbDeployOnDemand;
using DbDeployTools;
using System;
using System.Data.SqlClient;
using System.IO;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tests
{
    public class DatabaseFixture : IDisposable
    {
#if DEBUG
        private const string Configuration = "Debug";
#else
        private const string Configuration = "Release";
#endif

        private string DacpacPath { get; } = Path.GetFullPath($@"..\..\..\..\Database\bin\{Configuration}\Database.dacpac");

        public static string ConnectionString { get; } =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public static string DatabaseName { get; } = "Database8";

        private IMessageSink _output { get; }

        public DatabaseFixture(IMessageSink output)
        {
            _output = output;

            Log("Database fixture is starting.");
            Log("Connection string: {0}", ConnectionString);
            Log("Database name: {0}", DatabaseName);
            Log(".dacpac file: {0}", DacpacPath);

            DeployDatabaseIfNeeded();
        }

        private void Log(string message, params object[] args) =>
            _output.OnMessage(new DiagnosticMessage(message, args));

        private void DeployDatabaseIfNeeded()
        {
            Log("The database will now be deployed if the .dacpac has changed since the last deployment.", DacpacPath);

            DbDeployDecisionMaker.IfDatabaseNeedsDeploying(ConnectionString, DatabaseName, DacpacPath,
                () => DbDeployer.DeployDacPac(DacpacPath, ConnectionString, DatabaseName, logFunc: x => Log(x)));
        }

        public void ExecuteSql(string sql)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = $"USE [{DatabaseName}]; {sql}";

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
        }
    }
}