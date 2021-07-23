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
        /// Method to handle simulation State
        /// </summary>
        /// <param name="inbox"></param>
        /// <param name="hive"></param>
        private void Continuation()
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
                        Hive.Terminate().Wait();
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

        /// <summary>
        /// Adds the reference to the basic system to the StateManager
        /// </summary>
        /// <param name="hive"></param>
        /// <returns></returns>
        public IWithHive WithHive(Hive hive)
        {
            Hive = hive;
            return this;
        }

        /// <summary>
        /// Adds an Inbox to the configuration
        /// </summary>
        /// <param name="inbox">Mailbox that can be used to communicate with the stateManager</param>
        /// <returns></returns>
        public IWithInbox WithInbox(Inbox inbox)
        {
            Inbox = inbox;
            return this;
        }

        /// <summary>
        /// Starts the Actor system after Initialisation has been done.
        /// </summary>
        /// <returns></returns>
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
