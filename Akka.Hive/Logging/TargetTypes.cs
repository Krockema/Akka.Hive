namespace Akka.Hive.Logging
{
    /// <summary>
    /// Possible Logging Targets.
    /// </summary>
    public enum TargetTypes
    {
        /// <summary>
        /// Writes to Command line.
        /// </summary>
        Console,
        /// <summary>
        /// Writes to ../bin/.../Logs/*
        /// </summary>
        File,
        /// <summary>
        ///  Writes log messages to an ArrayList in memory for programmatic retrieval
        /// </summary>
        Memory,
        /// <summary>
        /// Writes to the debugging console
        /// </summary>
        Debugger
    }
}
