using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Logging;
using NLog;
using System;
using System.Web;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// Actor that regulates the Simulation Speed.
    /// </summary>
    public class StateManager :  ReceiveActor
    {
        private readonly NLog.Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        private bool IsRunning = false;
        private Hive _hive;
        internal protected IActorRef ContextManagerRef => _hive.ContextManagerRef;
        /// <summary>
        /// Creation method for HeartBeat Actor
        /// </summary>
        /// <param name="hive">Referenc to Hive</param>
        /// <returns></returns>
        public static Props Props(Hive hive)
        {
            return Actor.Props.Create(() => new StateManager(hive));
        }
        
        public StateManager(Hive hive) : base()
        {
            #region init
            _logger.Log(LogLevel.Info, "StateManager Created");
            _hive = hive;
            #endregion
            Receive<SimulationState>((state) => state == SimulationState.Finished, (_) => Finished());
            Receive<SimulationState>((state) => state == SimulationState.Started, (_) => Started());
            Receive<SimulationState>((state) => state == SimulationState.Stopped, (_) => Stopped());
            Receive<SimulationState>((state) => state == SimulationState.Bounced, (_) => Bounce());
            // anything unhandled
            ReceiveAny((message) => _logger.Log(LogLevel.Warn, "StateManager: Unhandled message -> " + message.GetType() + "recived!"));
        }

        public IActorRef GetActorFromString(string name)
        {
            return _hive.ActorSystem.ActorSelection(name).ResolveOne(TimeSpan.FromSeconds(60)).Result;
        }

        private void Stopped()
        {
            _logger.Log(LogLevel.Info, "Hive Stopped !");
            this.AfterSimulationStopped();
            _hive.Continue();
        }

        private void Finished()
        {
            _logger.Log(LogLevel.Info, "Hive Finished !");
            this.SimulationIsTerminating();
            Sender.Tell(new HiveMessage.Shutdown(Sender));
            IsRunning = false;
        }

        private void Started()
        {
            _logger.Log(LogLevel.Info, "Hive Started !");
            IsRunning = true;
            this.AfterSimulationStarted();
        }

        /// <summary>
        /// This is Called after the Systems Heart bounced. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void Bounce()
        {
            System.Diagnostics.Debug.WriteLine("Received Bounce !", "(AKKA:Hive)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void AfterSimulationStarted()
        {
            System.Diagnostics.Debug.WriteLine("Received simulation start!", "(AKKA:Hive)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void AfterSimulationStopped()
        {
            System.Diagnostics.Debug.WriteLine("Received simulation stop!", "(AKKA:Hive)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void SimulationIsTerminating()
        {
            System.Diagnostics.Debug.WriteLine("Received simulation finished!", "(AKKA:Hive)");
        }
    }
}

