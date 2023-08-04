using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Instructions;
using Akka.Hive.Interfaces;
using Akka.Hive.Logging;
using NLog;
using static Akka.Hive.Definitions.HiveMessage;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// Global Simulation Manager that manages the time, message processing, and heartbeat of the system. 
    /// </summary>
    public class SequenceManager : ReceiveActor
    {
        // for Normal Mode
        private readonly NLog.Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        private readonly IFeatureInstructions _featuredInstructions;
        private ICurrentInstructions _currentInstructions;

        /// <summary>
        /// Hive configuration
        /// </summary>
        private IHiveConfig EngineConfig { get; }

        /// <summary>
        /// Contains the next interval time where the simulation will stop.
        /// </summary>
        private Time NextInterrupt { get; set; }

        /// <summary>
        /// Set to true when the Simulation hase no events in Queue or has recieved a SimulationState.Finish message
        /// </summary>
        private bool IsComplete { get; set; } = false;
        /// <summary>
        /// Set to false when the Simulation recieved a Command.Stop, time will no longer advance.
        /// </summary>
        private bool IsRunning { get; set; } = false;

        /// <summary>
        /// Contains the current simulation Time
        /// </summary>
        private Time Time { get; set; }

        /// <summary>
        /// Contains the current simulation Time
        /// </summary>
        private Time EndTime { get; set; }

        /// <summary>
        /// Probe Constructor for Simulation context
        /// </summary>
        /// <returns>IActorRef of the SimulationContext</returns>
        public static Props Props(IHiveConfig config)
        {
            return Akka.Actor.Props.Create(() => new SequenceManager(config));
        }

        public SequenceManager(IHiveConfig config)
        {
            #region init
            EngineConfig = config;
            _featuredInstructions = InstructionStoreFactory.CreateFeatureStore(config.DebugHive);
            _currentInstructions = InstructionStoreFactory.CreateSequencialCurrent();
            Time = config.StartTime;
            EndTime = Time.Add(config.TimeSpanToTerminate);
            NextInterrupt = config.StartTime;
            #endregion init

            Become(SimulationMode);
        }
        /// <summary>
        /// Does not track Simulation Messages, only the amount that has to be processed
        /// </summary>
        private void SimulationMode()
        {

            Receive<SimulationCommand>(s => s == SimulationCommand.Start, s =>
            {
                if (_currentInstructions.Count() != 0)
                {
                    Context.Self.Forward(s);
                }
                else
                {
                    NextInterrupt = NextInterrupt.Add(EngineConfig.InterruptInterval);
                    EngineConfig.StateManagerRef.Tell(SimulationState.Started);
                    IsRunning = true;
                    Advance();
                }

            });

            // Determine when The Simulation is Done.
            Receive<SimulationCommand>(s => s == SimulationCommand.IsReady, s =>
            {
                //if (_InstructionStore.Count() == 0)
                if (_currentInstructions.Count() == 0)
                {
                    Sender.Tell(SimulationCommand.IsReady, ActorRefs.NoSender);
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Forward(s);
                }
            });

            // Determine when The Simulation is Done.
            Receive<SimulationState>(s => s == SimulationState.Finished, s =>
            {
                if (_currentInstructions.Count() == 0)
                {
                    EngineConfig.StateManagerRef.Tell(SimulationState.Finished);
                    IsComplete = true;
                    _logger.Log(LogLevel.Trace, "Simulation Finished...");
                }
                else
                {
                    // Not Ready Yet, Try Again
                    Context.Self.Forward(s);
                }
            });

            Receive<HiveMessage.Done>(c =>
            {
                var msg = ((IHiveMessage)c.Message);
                if (!_currentInstructions.Remove(msg: msg.Key))
                    throw new Exception("Failed to remove message from Instruction store");
                Advance();
            });

            Receive<SimulationCommand>(c => c == SimulationCommand.Stop, _ =>
            {
                // Console.WriteLine("-- Resume simulation -- !");
                _logger.Info("Command Stop --Done {arg1} Stop", new object[] { _currentInstructions.Count() });
                IsRunning = false;
            });

            Receive<Shutdown>(_ =>
            {
                if (_currentInstructions.Count() == 0 && _featuredInstructions.Count() == 0)
                {
                    CoordinatedShutdown.Get(Context.System)
                                       .Run(CoordinatedShutdown.ClrExitReason.Instance);
                }
            });

            Receive<Schedule>(m =>
            {
                if (_featuredInstructions.TryGetValue(m.AtTime.Value, out ICurrentInstructions instructions))
                    instructions.Add(m.Message.Key, m.Message);
                else
                {
                    instructions = InstructionStoreFactory.CreateSequencialCurrent();
                    instructions.Add(m.Message.Key, m.Message);
                    _featuredInstructions.Add(m.AtTime.Value, instructions);
                }
            });

            Receive<IHiveMessage>(m =>
            {
                if (m.Target == ActorRefs.NoSender)
                    throw new Exception("No Target Specified");

                //Console.WriteLine("++");
                // m = m.WithSender(Sender);
                _currentInstructions.Add(m.Key, m);
                _logger.Log(LogLevel.Trace, "Time[{arg1}] | {arg2} | DO ++ | Instructions: {arg2} | Type: {arg3} | Sender: {arg4} | Target: {arg5}"
                    , new object[] { Time.Value, m.Key, _currentInstructions.Count(), m.GetType().ToString(), Sender.Path.Name, m.Target.Path.Name });
            });
        }

        private void Advance()
        {
            if (_currentInstructions.Count() > 0)
                {
                    var m = _currentInstructions.GetNext();
                    m.Target.Tell(m, m.Sender);
                    return;
                }
            if (IsRunning && !IsComplete)
            {
                if (_featuredInstructions.Count() == 0 || _featuredInstructions.Next() >= EndTime.Value)
                {
                    IsComplete = true;
                    EngineConfig.StateManagerRef.Tell(SimulationState.Finished);
                }
                else
                {
                    Advance(_featuredInstructions.Next());
                }
            }
        }

        private void Advance(DateTime to)
        {
            // advance time
            if (Time.Value >= to)
                throw new Exception("Time cant be undone.");
            // stop sim at Interval
            if (Time.Value >= NextInterrupt.Value)
            {
                IsRunning = false;
                EngineConfig.StateManagerRef.Tell(SimulationState.Stopped);
                return;
            }
            Time = new Time(to);
            // get current Tasks
            MoveFeaturesToCurrentTimeSpan();

        }

        private void MoveFeaturesToCurrentTimeSpan()
        {
            if (_featuredInstructions.TryGetValue(Time.Value, out _currentInstructions))
            {
                _featuredInstructions.Remove(Time.Value);
                // global Tick
                var tick = new AdvanceTo(Time);
                Context.System.EventStream.Publish(tick);

                var m = _currentInstructions.GetNext();
                m.Target.Tell(m, m.Sender);
            }
        }
    }
}
