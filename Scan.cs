using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using _425show.SecretManager;
using System.Threading.Tasks;
using System.Linq;

namespace isolated_rotator
{
    public class Scanner
    {
        private readonly AppSecretManager _manager;
        private readonly ILogger _log;
        public Scanner(AppSecretManager manager, ILogger<Scanner> logger)
        {
            _manager = manager;
            _log = logger;
        }

        [Function(nameof(Scanner))]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timer, FunctionContext context)
        {
            _log.LogInformation($"{DateTime.UtcNow:o}: Looking for today's expiring credentials from storage");
            var todayKey = CredentialEntity.DerivePartitionKey(DateTime.UtcNow);
            _log.LogInformation($"Looking for key {todayKey}");
            var creds = await _manager.GetExpiringCredentials(todayKey.ToString());
            _log.LogInformation($"Got {creds.Count()} expiring credentials");
            _log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
            _log.LogInformation($"Next timer schedule at: {timer.ScheduleStatus.Next}");
        }
    }

    public class Synchronizer
    {
        private readonly AppSecretManager _manager;
        private readonly ILogger _log;
        public Synchronizer(AppSecretManager manager, ILogger<Scanner> logger)
        {
            _manager = manager;
            _log = logger;
        }

        [Function(nameof(Synchronizer))]
        public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer, FunctionContext context)
        {
            var creds = await _manager.GetCredentialMetadata();
            await _manager.PersistCredentialMetadata(creds);
            _log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _log.LogInformation($"Next timer schedule at: {timer.ScheduleStatus.Next}");
        }
    }

    public class TimerInfo
    {
        public TimerScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class TimerScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
