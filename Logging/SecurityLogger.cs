using System.Text.Json;

namespace SecurePasswordManager.Logging
{
    public class SecurityLogger
    {
        private readonly string _logPath;

        public SecurityLogger(string logPath)
        {
            _logPath = logPath;
        }

        public void Log(string eventType, string outcome)
        {
            var logEntry = new
            {
                TimestampUtc = DateTime.UtcNow,
                EventType = eventType,
                Outcome = outcome
            };

            string json = JsonSerializer.Serialize(logEntry);

            File.AppendAllText(
                _logPath,
                json + Environment.NewLine
            );
        }
    }
}