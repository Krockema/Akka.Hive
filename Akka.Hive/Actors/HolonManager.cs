using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;
using Akka.Hive.Logging;
using NLog;
using static Akka.Hive.Definitions.HiveMessage;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// ... is the basic implementation of the supervising actor.
    /// </summary>
    public class HolonManager : ReceiveActor, IWithTimers
    {
      
        private readonly NLog.Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);

        private IHiveConfig EngineConfig { get; }

        // local scheduler for Timed events
        public ITimerScheduler Timers { get; set; }

        /// <summary>
        /// Contains the current simulation Time
        /// </summary>
        private Time Time { get; set; }
        
        /// <summary>
        /// Probe Constructor for Simulation context
        /// </summary>
        /// <returns>IActorRef of the SimulationContext</returns>
        public static Props Props(IHiveConfig config)
        {
            return Akka.Actor.Props.Create(() => new HolonManager(config));
        }
        
        /// <summary>
        /// Creates a Actor 'Holon Manager' That acts as basic supervisor
        /// Used only for Message forwarding and shutdown coordination
        /// </summary>
        /// <param name="config">IHiveConfig object</param>
        public HolonManager(IHiveConfig config)
        {
            #region init
            EngineConfig = config;
            Time = config.StartTime;
            #endregion init

            Receive<SimulationCommand>(s => s == SimulationCommand.Start, s =>
            {
                Timers.StartPeriodicTimer("Bounce", SimulationCommand.HeartBeat, EngineConfig.InterruptInterval, EngineConfig.InterruptInterval);
                EngineConfig.StateManagerRef.Tell(SimulationState.Started);
            });

            Receive<SimulationCommand>(s => s == SimulationCommand.HeartBeat, s =>
            {
                EngineConfig.StateManagerRef.Tell(SimulationState.Bounced);
            });

            // Determine when The Simulation is Done.
            Receive<SimulationCommand>(s => s == SimulationCommand.IsReady, s =>
            {
                Sender.Tell(SimulationCommand.IsReady, ActorRefs.NoSender);
            });

            Receive<SimulationCommand>(c => c == SimulationCommand.Stop, c =>
            {
                // Console.WriteLine("-- Resume simulation -- !");
                _logger.Info("Command Stop from {}", Sender.Path.Name);
            });

            Receive<Shutdown>(c =>
            {
                CoordinatedShutdown.Get(Context.System)
                    .Run(CoordinatedShutdown.ClrExitReason.Instance);
            });

            Receive<IHiveMessage>(m =>
            {
                m.Target.Forward(m);
            });
        }
    }
}
