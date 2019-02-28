using System;
using System.Data.SqlClient;
using System.IO;

namespace DbDeployOnDemand
{
    public static class DbDeployDecisionMaker
    {
        public static void IfDatabaseNeedsDeploying(string connectionString, string database,
    string dacpacPath, Action action)
        {
            var deployedHash = ReadLastDacpacBuildTime(connectionString, database);
            var currentHash = GetLastDacpacBuildTime(dacpacPath);

            if (deployedHash != currentHash)
            {
                action();
                UpdateLastDacpacBuildTime(connectionString, database, currentHash);
            }
        }

        private static DateTimeOffset? ReadLastDacpacBuildTime(string connectionString, string database)
        {
            try
            {
                using (var connection =
                    new SqlConnection(connectionString))
                {
                    var sql = $"USE [{database}]; SELECT TOP(1) [LastDeployedDacpacBuildTime] FROM [DbDeployMetadata].[Deployment];";

                    var command = new SqlCommand(sql, connection);

                    connection.Open();
                    var x = command.ExecuteScalar();
                    return x as DateTimeOffset?;
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private static void UpdateLastDacpacBuildTime(string connectionString, string database, DateTimeOffset? timestamp)
        {
            using (var connection =
                new SqlConnection(connectionString))
            {
                var sql =
                    $@"
USE [{database}];
IF NOT EXISTS (SELECT name FROM sys.schemas WHERE name = N'DbDeployMetadata')
BEGIN
    EXEC('CREATE SCHEMA [DbDeployMetadata]');
END;

IF OBJECT_ID(N'[DbDeployMetadata].[Deployment]', N'U') IS NULL
BEGIN
    CREATE TABLE [DbDeployMetadata].[Deployment] ([Version] INT NOT NULL, [LastDeployedDacpacBuildTime] DATETIMEOFFSET NOT NULL);
END;

BEGIN TRANSACTION
    DELETE FROM [DbDeployMetadata].[Deployment];
    INSERT INTO [DbDeployMetadata].[Deployment] ([Version], [LastDeployedDacpacBuildTime]) VALUES (1, @Timestamp);
COMMIT;";

                var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("Timestamp", timestamp);
                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        private static DateTimeOffset? GetLastDacpacBuildTime(string dacpacPath)
        {
            return File.GetLastWriteTimeUtc(dacpacPath);
        }
    }
}