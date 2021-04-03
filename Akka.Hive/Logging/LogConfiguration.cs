using NLog;
using NLog.Config;
using System;
using NLog.Targets;

namespace Akka.Hive.Logging
{
    public class LogConfiguration
    {
        // ${date:format=HH\:mm\:ss}|
        private const string LogLayout =  @"[${logger}][${level:uppercase=true}] ${message}";

        private static readonly LoggingConfiguration Config = new ();

        public static void LogTo(TargetTypes targetType, string target, LogLevel logLevel, bool final = false,
            string logFilter = "*")
        {
            LogTo(targetType, target, logLevel, logLevel);
        }

        public static void LogTo(TargetTypes targetType, string target, LogLevel minLevel, LogLevel maxLevel, bool final = false, string logFilter = "*")
        {
           
            Target loggingTarget;
            switch (targetType)
            {
                case TargetTypes.File:
                    loggingTarget = new FileTarget(target)
                        {
                            FileName = "${basedir}/Logs/" + target + ".log"
                            ,Layout = LogLayout
                            , KeepFileOpen = true
                            , ArchiveOldFileOnStartup = true
                            , ArchiveAboveSize = 45000000
                    };
                    break;

                case TargetTypes.Console:
                    loggingTarget = new ColoredConsoleTarget(target)
                        {
                            Layout = LogLayout
                        };
                    break;
                    
                case TargetTypes.Memory:
                    loggingTarget = new MemoryTarget(target)
                        {
                            Layout = LogLayout
                        };
                    break;

                case TargetTypes.Debugger:
                    loggingTarget = new DebuggerTarget(target)
                    {
                        Layout = LogLayout
                    };
                    break;
                default: throw new Exception("No valid logging target found.");
            }

            // Step 3. Set target properties 
            Config.AddTarget(target, loggingTarget);

            // Step 4. Define rules
            var rule = new LoggingRule($"*{target}*", minLevel, maxLevel, loggingTarget)
            {
                Final = false
            };

            Config.LoggingRules.Add(rule);

            // Step 5. Activate the configuration
            LogManager.Configuration = Config;
        }

    }


}