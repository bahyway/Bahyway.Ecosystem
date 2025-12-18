using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace AlarmInsight.Infrastructure.Services
{
    public interface IPostgreSQLHealthService
    {
        Task<Dictionary<string, object>> GetClusterHealthAsync(bool includeHAProxy = false, bool includeBarman = false);

        Task<Dictionary<string, object>> TestDockerEnvironmentAsync();
        Task<Dictionary<string, object>> TestPrimaryNodeAsync(string containerName = null);
        Task<Dictionary<string, object>> TestReplicaNodeAsync(string containerName = null);
        Task<Dictionary<string, object>> TestReplicationStatusAsync();
        Task<List<Dictionary<string, object>>> GetHealthAlarmsAsync();

        Task<Collection<PSObject>> InvokePowerShellAsync(
            string command,
            Dictionary<string, object> parameters = null);
    }

    public class PostgreSQLHealthService : IPostgreSQLHealthService, IDisposable
    {
        private readonly ILogger<PostgreSQLHealthService> _logger; private readonly string _modulePath; private Runspace _runspace; private bool _disposed = false;

        public PostgreSQLHealthService(ILogger<PostgreSQLHealthService> logger)
        {
            _logger = logger;
            _modulePath = GetModulePath();
            InitializeRunspace();
        }

        private string GetModulePath()
        {
            // Option 1: Module in output directory (deployment)
            var outputPath = Path.Combine(
                AppContext.BaseDirectory,
                "PowerShellModules",
                "BahyWay.PostgreSQLHA",
                "BahyWay.PostgreSQLHA.psd1"
            );

            if (File.Exists(outputPath))
            {
                _logger.LogInformation($"Found PowerShell module at: {outputPath}");
                return outputPath;
            }

            // Option 2: Module in repository (development)
            var repoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..",
                "infrastructure",
                "postgresql-ha",
                "powershell-module",
                "BahyWay.PostgreSQLHA",
                "BahyWay.PostgreSQLHA.psd1"
            );

            if (File.Exists(repoPath))
            {
                _logger.LogInformation($"Found PowerShell module at: {repoPath}");
                return Path.GetFullPath(repoPath);
            }

            throw new FileNotFoundException(
                "PowerShell module not found. Searched: " +
                $"{outputPath}, {repoPath}"
            );
        }

        private void InitializeRunspace()
        {
            try
            {
                var initialSessionState = InitialSessionState.CreateDefault();
                initialSessionState.ExecutionPolicy =
                    Microsoft.PowerShell.ExecutionPolicy.RemoteSigned;

                _runspace = RunspaceFactory.CreateRunspace(initialSessionState);
                _runspace.Open();

                using var pipeline = _runspace.CreatePipeline();
                pipeline.Commands.AddScript($"Import-Module '{_modulePath}' -Force");
                pipeline.Invoke();

                _logger.LogInformation("PowerShell runspace initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize PowerShell runspace");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetClusterHealthAsync(
            bool includeHAProxy = false,
            bool includeBarman = false)
        {
            var parameters = new Dictionary<string, object>();
            if (includeHAProxy) parameters["IncludeHAProxy"] = true;
            if (includeBarman) parameters["IncludeBarman"] = true;

            var result = await InvokePowerShellAsync("Get-ClusterHealth", parameters);
            return ConvertPSObjectToDictionary(result.FirstOrDefault());
        }

        public async Task<Dictionary<string, object>> TestDockerEnvironmentAsync()
        {
            var result = await InvokePowerShellAsync("Test-DockerEnvironment");
            return ConvertPSObjectToDictionary(result.FirstOrDefault());
        }

        public async Task<Dictionary<string, object>> TestPrimaryNodeAsync(
            string containerName = null)
        {
            var parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(containerName))
                parameters["ContainerName"] = containerName;

            var result = await InvokePowerShellAsync("Test-PostgreSQLPrimary", parameters);
            return ConvertPSObjectToDictionary(result.FirstOrDefault());
        }

        public async Task<Dictionary<string, object>> TestReplicaNodeAsync(
            string containerName = null)
        {
            var parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(containerName))
                parameters["ContainerName"] = containerName;

            var result = await InvokePowerShellAsync("Test-PostgreSQLReplica", parameters);
            return ConvertPSObjectToDictionary(result.FirstOrDefault());
        }

        public async Task<Dictionary<string, object>> TestReplicationStatusAsync()
        {
            var result = await InvokePowerShellAsync("Test-PostgreSQLReplication");
            return ConvertPSObjectToDictionary(result.FirstOrDefault());
        }

        public async Task<List<Dictionary<string, object>>> GetHealthAlarmsAsync()
        {
            var result = await InvokePowerShellAsync("Get-HealthAlarms");
            return result.Select(ConvertPSObjectToDictionary).ToList();
        }

        public async Task<Collection<PSObject>> InvokePowerShellAsync(
            string command,
            Dictionary<string, object> parameters = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var pipeline = _runspace.CreatePipeline();
                    var cmd = new Command(command);

                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param.Key, param.Value);
                        }
                    }

                    pipeline.Commands.Add(cmd);
                    _logger.LogDebug($"Executing PowerShell: {command}");

                    var results = pipeline.Invoke();

                    if (pipeline.Error.Count > 0)
                    {
                        var errors = pipeline.Error.ReadToEnd()
                            .Select(e => e.ToString()).ToList();
                        _logger.LogWarning(
                            $"PowerShell '{command}' completed with errors: " +
                            string.Join("; ", errors)
                        );
                    }

                    return results;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error executing PowerShell: {command}");
                    throw;
                }
            });
        }

        private Dictionary<string, object> ConvertPSObjectToDictionary(PSObject psObject)
        {
            if (psObject == null)
                return new Dictionary<string, object>();

            var dict = new Dictionary<string, object>();

            foreach (var property in psObject.Properties)
            {
                var value = property.Value;

                if (value is PSObject nestedPsObject)
                {
                    value = ConvertPSObjectToDictionary(nestedPsObject);
                }
                else if (value is object[] array)
                {
                    value = array.Select(item =>
                        item is PSObject pso
                            ? ConvertPSObjectToDictionary(pso)
                            : item
                    ).ToList();
                }

                dict[property.Name] = value;
            }

            return dict;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _runspace?.Dispose();
                    _logger.LogInformation("PowerShell runspace disposed");
                }
                _disposed = true;
            }
        }

        ~PostgreSQLHealthService()
        {
            Dispose(false);
        }
    }
}