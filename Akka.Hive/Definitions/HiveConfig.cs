using System;
using Akka.Actor;
using Akka.Hive.Action;

namespace Akka.Hive.Definitions
{
    /// <summary>
    /// Global Akka Hive Configuration, containing start time, interrupt interval, simulation break, actor action factory, and debug switches.
    /// </summary>
    public class HiveConfig
    {
        /// <summary>
        /// Global Akka Hive Configuration 
        /// </summary>
        /// <param name="debugAkka">Debug the Akka Core System</param>
        /// <param name="debugHive">Debug Akka driven engine for Simulation and Realtime </param>
        /// <param name="interruptInterval">At what TimeSpan shall the system stop and wait for further Commands
        ///                                 the System will continue by calling the SimulationContext.Continue() method.</param>
        /// <param name="timeToAdvance">minimum time to Advance to advance the simulation clock</param>
        /// <param name="startTime"></param>
        /// <param name="actorActionFactory"></param>
        private HiveConfig(bool debugAkka, bool debugHive, TimeSpan interruptInterval, TimeSpan timeToAdvance, Time startTime,  ActionFactory actorActionFactory)
        {
            InterruptInterval = interruptInterval;
            DebugAkka = debugAkka;
            DebugHive = debugHive;
            TimeToAdvance = timeToAdvance;
            StartTime = startTime;
            ActorActionFactory = actorActionFactory;
        }

        /// <summary>
        /// Creates Akka Hive Configuration for Simulation purpose.
        /// </summary>
        /// <param name="debugAkka">Debug the Akka Core System</param>
        /// <param name="debugHive">Debug Akka Hive for simulation and normal time</param>
        /// <param name="interruptInterval">At what TimeSpan shall the system stop and wait for further Commands
        ///                                 the System will continue by calling the SimulationContext.Continue() method.</param>
        /// <param name="timeToAdvance">Minimum time span to Advance to advance the simulation clock</param>
        /// <param name="startTime">Start time of the simulation</param>
        /// <returns>HiveConfig</returns>
        public static HiveConfig CreateSimulationConfig(bool debugAkka, bool debugHive, TimeSpan interruptInterval,
            TimeSpan timeToAdvance, Time startTime)
        {
            return new HiveConfig(debugAkka, debugHive, interruptInterval, timeToAdvance, startTime, new ActionFactory());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="debugAkka"></param>
        /// <param name="debugHive"></param>
        /// <param name="interruptInterval"></param>
        /// <param name="startTime">Start time of the system</param>
        /// <param name="actorActionFactory">Actor action factory for custom Holon functionality</param>
        /// <returns>HiveConfig</returns>
        public static HiveConfig CreateHolonConfig(bool debugAkka, bool debugHive, TimeSpan interruptInterval,
            Time startTime, ActionFactory actorActionFactory)
        {
            return new HiveConfig(debugAkka, debugHive, interruptInterval, TimeSpan.Zero, startTime, actorActionFactory);
        }

        public TimeSpan TimeToAdvance { get; }
        public Time StartTime { get; }
        public TimeSpan InterruptInterval { get; }
        public bool DebugAkka { get; }
        public bool DebugHive { get; }
        public ActionFactory ActorActionFactory { get; }
        public Inbox Inbox { get; set; }
    }
}

