using System;
using Akka.Actor;
using Akka.Hive.Action;

namespace Akka.Hive.Definitions
{
    /// <summary>
    /// Global Akka Hive Configuration, containing start time, interrupt interval, simulation break, actor action factory, and debug switches.
    /// </summary>
    public record HiveConfig: IHiveConfig, IHiveConfigBase, IHiveConfigHolon, IHiveConfigSimulation
    {
        private HiveConfig()
        {
            InterruptInterval = TimeSpan.MaxValue;
            DebugAkka = false;
            DebugHive = false;
            TickSpeed = TimeSpan.Zero;
            StartTime = Time.Now;
            ActorActionFactory = new ActionFactory();
            MessageTrace = new MessageTrace();
        }

        /// <summary>
        /// Time at wich the system start to calculate.
        /// </summary>
        /// <param name="startTime">Default is Time.Now</param>
        /// <returns></returns>
        public IHiveConfigBase WithStartTime(Time startTime)
        {
            return this with { StartTime = startTime };
        }

        /// <summary>
        /// Creates basic Hive Configuration
        /// </summary>
        /// <returns></returns>
        public static IHiveConfigSimulation ConfigureSimulation()
        {
            return new HiveConfig();
        }

        /// <summary>
        /// Creates basic Hive Configuration Holonic message flow
        /// </summary>
        /// <returns></returns>
        public static IHiveConfigHolon ConfigureHolon()
        {
            return new HiveConfig();
        }

        /// <summary>
        /// Transforms from HiveConfigBase to HiveConfig
        /// </summary>
        /// <returns></returns>
        public IHiveConfig Build() => this;

        /// <summary>
        /// Debugging
        /// </summary>
        /// <param name="akka"> akka internal using nLog </param>
        /// <param name="hive"> hive internal unsing nLog </param>
        /// <returns></returns>
        public IHiveConfigBase WithDebugging(bool akka, bool hive)
        {
            return this with { DebugAkka = akka, DebugHive = hive};
        }

        /// <summary>
        /// TimeSpan in wich the SimulationStateManager is triggered
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public IHiveConfigBase WithInterruptInterval(TimeSpan timeSpan)
        {
            return this with { InterruptInterval =  timeSpan };
        }

        /// <summary>
        /// Configures the simulation runspeed for each tick.
        /// </summary>
        /// <param name="timeSpan">Default is TimeSpan.Zero</param>
        /// <returns></returns>
        public IHiveConfigBase WithTickSpeed(TimeSpan timeSpan)
        {
            return this with { TickSpeed = timeSpan };
        }

        /// <summary>
        /// Configurates the Actor Action Factory for Holonic Apporach
        /// </summary>
        /// <param name="actionFactory"></param>
        /// <returns></returns>
        public IHiveConfigBase WithActionFactory(ActionFactory actionFactory)
        {
            return this with { ActorActionFactory = actionFactory };
        }


        /// <summary>
        /// Enables Message Tracing
        /// </summary>
        /// <returns></returns>
        public IHiveConfigBase WithMessageTracer(MessageTrace tracer)
        {
            return this with { MessageTrace = tracer  };
        }

        /// <summary>
        /// Terminates the simulation at (DateTime)Start + (TimeSpan)End
        /// it will execute the current time step
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public IHiveConfigBase WithTimeSpanToTerminate(TimeSpan timeSpanToTerminate)
        {
            return this with { TimeSpanToTerminate = timeSpanToTerminate };
        }

        public TimeSpan TimeSpanToTerminate { get; init; }
        public TimeSpan TickSpeed { get; init; }
        public Time StartTime { get; init; }
        public TimeSpan InterruptInterval { get; init; }
        public bool DebugAkka { get; init; }
        public bool DebugHive { get; init; }
        public ActionFactory ActorActionFactory { get; init; }
        public MessageTrace MessageTrace { get; init; }
        public Inbox Inbox { get; set; }
    }
}

