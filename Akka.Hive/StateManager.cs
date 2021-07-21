using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;
using Akka.Hive.Logging;
using NLog;

namespace Akka.Hive
{
    /// <summary>
    /// ... is listening to the Hive.Inbox and reacts on simulation start, stop, shutdown and heart beat bounce.
    /// </summary>
    public class StateManager: IStateManagerBase, IWithInbox, IWithHive
    {
        protected StateManager() { }

        private readonly NLog.Logger Logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        private bool IsRunning = true;
        private Inbox Inbox;
        private Hive Hive;
        /// <summary>
        /// Method do handle simulation State
        /// </summary>
        /// <param name="inbox"></param>
        /// <param name="hive"></param>
        public void Continuation()
        {
            while (IsRunning)
            {
                var message = Inbox.ReceiveAsync(timeout: TimeSpan.FromHours(value: 1)).Result;
                switch (message)
                {
                    case HiveMessage.SimulationState.Started:
                        Logger.Log(LogLevel.Warn, "Hive Started !");
                        this.AfterSimulationStarted();
                        Continuation();
                        break;
                    case HiveMessage.SimulationState.Stopped:
                        Logger.Log(LogLevel.Warn, "Hive Stopped !");
                        this.AfterSimulationStopped();
                        Hive.Continue();
                        Continuation();
                        break;
                    case HiveMessage.SimulationState.Finished:
                        Logger.Log(LogLevel.Warn, "Hive Finished !");
                        this.SimulationIsTerminating();
                        Hive.ActorSystem.Terminate().Wait();
                        IsRunning = false;
                        break;
                    case HiveMessage.SimulationState.Bounced:
                        Logger.Log(LogLevel.Warn, "Heart Bounced !");
                        this.Bounce();
                        break;
                    default:
                        Logger.Log(LogLevel.Warn, "StateManager: Unhandled message -> " + message.GetType() + "recived!");
                        break;
                }
            }
        }

        public static IStateManagerBase Base => new StateManager();
        public IWithHive WithHive(Hive hive)
        {
            Hive = hive;
            return this;
        }
        public IWithInbox WithInbox(Inbox inbox)
        {
            Inbox = inbox;
            return this;
        }
        public StateManager Start()
        {
            Continuation();
            return this;
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
