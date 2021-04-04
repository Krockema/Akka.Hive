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

        private HiveConfig EngineConfig { get; }
        public ITimerScheduler Timers { get; set; }

        /// <summary>
        /// Contains the current simulation Time
        /// </summary>
        private Time Time { get; set; }
        
        /// <summary>
        /// Probe Constructor for Simulation context
        /// </summary>
        /// <returns>IActorRef of the SimulationContext</returns>
        public static Props Props(HiveConfig config)
        {
            return Akka.Actor.Props.Create(() => new HolonManager(config));
        }

        public HolonManager(HiveConfig config)
        {
            #region init
            EngineConfig = config;
            Time = config.StartTime;
            #endregion init

            Receive<Command>(s => s == Command.Start, s =>
            {
                Timers.StartPeriodicTimer("Bounce", Command.HeartBeat, EngineConfig.InterruptInterval, EngineConfig.InterruptInterval);
                EngineConfig.Inbox.Receiver.Tell(SimulationState.Started);
            });

            Receive<Command>(s => s == Command.HeartBeat, s =>
            {
                EngineConfig.Inbox.Receiver.Tell(SimulationState.Bounced);
            });

            // Determine when The Simulation is Done.
            Receive<Command>(s => s == Command.IsReady, s =>
            {
                Sender.Tell(Command.IsReady, ActorRefs.NoSender);
            });

            Receive<Command>(c => c == Command.Stop, c =>
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
