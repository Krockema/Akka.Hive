using NLog;

namespace Akka.Hive.Logging
{
    public class LogDetail
    {
        public string TargetName { get; set; }
        public TargetTypes TargetTypes { get; set; }
        public LogLevel LogLevel { get; set; }
        public string FilterPattern { get; set; }
    }
}
