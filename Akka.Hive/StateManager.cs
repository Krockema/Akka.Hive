using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Logging;
using NLog;

namespace Akka.Hive
{
    /// <summary>
    /// ... is listening to the Hive.Inbox and reacts on simulation start, stop, shutdown and heart beat bounce.
    /// </summary>
    public class StateManager
    {
        private readonly NLog.Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        private bool _isRunning = true;
        /// <summary>
        /// Method do handle simulation State
        /// </summary>
        /// <param name="inbox"></param>
        /// <param name="hive"></param>
        public void Continuation(Inbox inbox, Hive hive)
        {
            while (_isRunning)
            {
                var message = inbox.ReceiveAsync(timeout: TimeSpan.FromHours(value: 1)).Result;
                switch (message)
                {
                    case HiveMessage.SimulationState.Started:
                        _logger.Log(LogLevel.Warn, "Hive Started !");
                        this.AfterSimulationStarted(hive);
                        Continuation(inbox: inbox, hive: hive);
                        break;
                    case HiveMessage.SimulationState.Stopped:
                        _logger.Log(LogLevel.Warn, "Hive Stopped !");
                        this.AfterSimulationStopped(hive);
                        hive.Continue();
                        Continuation(inbox, hive);
                        break;
                    case HiveMessage.SimulationState.Finished:
                        _logger.Log(LogLevel.Warn, "Hive Finished !");
                        this.SimulationIsTerminating(hive);
                        hive.ActorSystem.Terminate().Wait();
                        _isRunning = false;
                        break;
                    case HiveMessage.SimulationState.Bounced:
                        _logger.Log(LogLevel.Warn, "Heart Bounced !");
                        this.Bounce(hive);
                        break;
                    default:
                        _logger.Log(LogLevel.Warn, "StateManager: Unhandled message -> " + message.GetType() + "recived!");
                        break;
                }
            }
        }

        /// <summary>
        /// This is Called after the Systems Heart bounced. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void Bounce(Hive engine)
        {
            System.Diagnostics.Debug.WriteLine("Received Bounce !", "(AKKA:Hive)");
        }


        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void AfterSimulationStarted(Hive engine)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation start!", "(AKKA:Hive)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void AfterSimulationStopped(Hive engine)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation stop!", "(AKKA:Hive)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void SimulationIsTerminating(Hive engine)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation finished!", "(AKKA:Hive)");
        }
    }
}
