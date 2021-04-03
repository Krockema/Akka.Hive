using System;
using Akka.Actor;
using Akka.Hive.Action;

namespace Akka.Hive.Definitions
{
    public class HiveConfig
    {
        /// <summary>
        /// Global Akka Engine Configuration 
        /// </summary>
        /// <param name="debugAkka">Debug the Akka Core System</param>
        /// <param name="debugHive">Debug Akka driven engine for Simulation and Realtime </param>
        /// <param name="interruptInterval">At what TimeSpan shall the system stop and wait for further Commands
        ///                                 the System will continue by calling the SimulationContext.Coninue() method.</param>
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

        public static HiveConfig CreateSimulationConfig(bool debugAkka, bool debugEngine, TimeSpan interruptInterval,
            TimeSpan timeToAdvance, Time startTime)
        {
            return new HiveConfig(debugAkka, debugEngine, interruptInterval, timeToAdvance, startTime, new ActionFactory());
        }

        public static HiveConfig CreateHolonConfig(bool debugAkka, bool debugEngine, TimeSpan interruptInterval,
            TimeSpan timeToAdvance, Time startTime, ActionFactory actorActionFactory)
        {
            return new HiveConfig(debugAkka, debugEngine, interruptInterval, timeToAdvance, startTime, actorActionFactory);
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

