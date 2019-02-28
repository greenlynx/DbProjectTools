using Microsoft.SqlServer.Dac;
using System;
using System.Diagnostics;

namespace DbDeployTools
{
    public static class DbDeployer
    {
        public static void DeployDacPac(string dacPacPath, string connectionString, string databaseName, DacDeployOptions options = null, Action<string> logFunc = null)
        {
            if (options == null)
            {
                options = new DacDeployOptions
                {
                    BlockOnPossibleDataLoss = false,
                };
            }

            if (logFunc == null)
            {
                logFunc = x => Debug.WriteLine(x);
            }

            var dacServiceInstance = new DacServices(connectionString);
            dacServiceInstance.ProgressChanged +=
                new EventHandler<DacProgressEventArgs>((s, e) =>
                    logFunc(e.Message));
            dacServiceInstance.Message +=
                new EventHandler<DacMessageEventArgs>((s, e) =>
                    logFunc(e.Message.Message));

            using (var dacpac = DacPackage.Load(dacPacPath))
            {
                dacServiceInstance.Deploy(dacpac, databaseName,
                    upgradeExisting: true,
                    options: options);
            }
        }
    }
}