using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Logging;
using NLog;

namespace Akka.Hive
{
    public class StateManager
    {
        private readonly NLog.Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
        private bool _isRunning = true;
        /// <summary>
        /// Method do handle simulation State
        /// </summary>
        /// <param name="inbox"></param>
        /// <param name="engine"></param>
        public void Continuation(Inbox inbox, Hive engine)
        {
            while (_isRunning)
            {
                var message = inbox.ReceiveAsync(timeout: TimeSpan.FromHours(value: 1)).Result;
                switch (message)
                {
                    case HiveMessage.SimulationState.Started:
                        _logger.Log(LogLevel.Warn, "Sim Started !");
                        this.AfterSimulationStarted(engine);
                        Continuation(inbox: inbox, engine: engine);
                        break;
                    case HiveMessage.SimulationState.Stopped:
                        _logger.Log(LogLevel.Warn, "Sim Stopped !");
                        this.AfterSimulationStopped(engine);
                        engine.Continue();
                        Continuation(inbox, engine);
                        break;
                    case HiveMessage.SimulationState.Finished:
                        _logger.Log(LogLevel.Warn, "Sim Finished !");
                        this.SimulationIsTerminating(engine);
                        engine.ActorSystem.Terminate().Wait();
                        _isRunning = false;
                        break;
                    case HiveMessage.SimulationState.Bounced:
                        _logger.Log(LogLevel.Warn, "Heart Bounced !");
                        this.Bounce(engine);
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
            System.Diagnostics.Debug.WriteLine("Received Bounce !", "(AKKA:SIM)");
        }


        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void AfterSimulationStarted(Hive engine)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation start!", "(AKKA:SIM)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void AfterSimulationStopped(Hive engine)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation stop!", "(AKKA:SIM)");
        }

        /// <summary>
        /// This is Called after the Simulation started. Does nothing on default.
        /// You can overwrite this to 
        /// </summary>
        public virtual void SimulationIsTerminating(Hive engine)
        {
            System.Diagnostics.Debug.WriteLine("Received simulation finished!", "(AKKA:SIM)");
        }
    }
}
