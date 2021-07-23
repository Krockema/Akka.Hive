using NLog;
using NLog.Config;
using System;
using NLog.Targets;

namespace Akka.Hive.Logging
{
    /// <summary>
    /// Creates Logging configuration based on the LogDetails.
    /// </summary>
    public class LogConfiguration
    {
        // ${date:format=HH\:mm\:ss}|
        private const string LogLayout =  @"[${logger}][${level:uppercase=true}] ${message}";

        private static readonly LoggingConfiguration Config = new ();

        /// <summary>
        /// Creates a custom logging target
        /// </summary>
        /// <param name="targetType">Akka.Hive.Logging.TargetType [Console, File, Memmory, Debugger]</param>
        /// <param name="target">target directory below $/{basedir}/Logs/</param>
        /// <param name="logLevel">Nlog.LogLevel</param>
        /// <param name="final">default false</param>
        /// <param name="logFilter">default "*"</param>
        public static void LogTo(TargetTypes targetType, string target, LogLevel logLevel, bool final = false,
            string logFilter = "*")
        {
            LogTo(targetType, target, logLevel, logLevel);
        }

        /// <summary>
        /// Creates a custom logging target
        /// </summary>
        /// <param name="targetType">Akka.Hive.Logging.TargetType [Console, File, Memmory, Debugger]</param>
        /// <param name="target">target directory below $/{basedir}/Logs/</param>
        /// <param name="minLevel">Nlog.LogLevel</param>
        /// <param name="maxLevel">Nlog.LogLevel</param>
        /// <param name="final">default false</param>
        /// <param name="logFilter">default "*"</param>
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