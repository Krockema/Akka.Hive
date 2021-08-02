using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Hive.Action;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using static Akka.Hive.Definitions.HiveMessage;

namespace Akka.Hive
{
    /// <summary>
    /// ... creates and holds the Actor System, Hive Configuration, Context Manager. Loads the Akka Configuration and provides basic functionality.
    /// </summary>
    public class Hive
    {
        /// <summary>
        /// Public Contextname for identification
        /// </summary>
        public const string ContextName = "HiveContext";
        /// <summary>
        /// The actor system itself
        /// </summary>
        public ActorSystem ActorSystem { get; }

        /// <summary>
        /// The hive Configuration
        /// </summary>
        public IHiveConfig Config { get; }
        /// <summary>
        /// Global Inbox that is used for comunication with the state manager
        /// </summary>
        public Inbox Inbox => Config.Inbox;
        /// <summary>
        /// Actor managing the virtual time, is triggered by interupts and can be used for external comunication
        /// </summary>
        public IActorRef ContextManager { get; }
        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="engineConfig">Several Simulation Configurations</param>
        public Hive(IHiveConfig engineConfig)
        {
            Config config = (engineConfig.DebugAkka) ? ConfigurationFactory.ParseString(GetConfiguration(NLog.LogLevel.Debug)) 
                                       /* else */ : ConfigurationFactory.ParseString(GetConfiguration(NLog.LogLevel.Info));

            ActorSystem  = ActorSystem.Create(ContextName, config);
            
            // ********* Not Ready Yet ******************* /// 
            //
            // if(simConfig.AddApplicationInsights)
            // {
            //    var monitor = new ActorAppInsightsMonitor(SimulationContextName);
            //    var monitorExtension = ActorMonitoringExtension.RegisterMonitor(ActorSystem, monitor);
            // }
            Config = engineConfig;
            engineConfig.Inbox =  Inbox.Create(ActorSystem);
            ContextManager = CreateContextRef(engineConfig);

        }

        private IActorRef CreateContextRef(IHiveConfig engineConfig)
        {
            return engineConfig.ActorActionFactory.ActorActions switch
            {
                ActionsType.Simulation => ActorSystem.ActorOf(SimulationManager.Props(engineConfig), ContextName),
                ActionsType.Holon =>  ActorSystem.ActorOf(HolonManager.Props(engineConfig), ContextName),
                _ => throw new NotImplementedException(),
            };
        }
        
        /// <summary>
        /// Request a ready signal from system context
        /// </summary>
        /// <returns>true as soon as the initialization is finished</returns>
        public bool IsReady()
        {
            var r = ContextManager.Ask(Command.IsReady).Result;
            return r is Command.IsReady;
        }
        /// <summary>
        /// Confiniues the system after a stop.
        /// Only on simulation contexts
        /// </summary>
        public void Continue() {
            ContextManager.Tell(Command.Start);
        }

        /// <summary>
        /// Terminates the ActorSystem.
        /// </summary>
        /// <returns>Task when the system is terminated</returns>
        public Task Terminate()
        {
            return ActorSystem.Terminate();
        }

        /// <summary>
        /// Starts the actor system
        /// </summary>
        /// <returns>when the system is terminated</returns>
        public Task RunAsync()
        {
            ContextManager.Tell(Command.Start);
            return ActorSystem.WhenTerminated;
        }

        /// <summary>
        /// should later read from app.config or dynamically created
        /// </summary>
        /// <returns></returns>
        private string GetConfiguration(NLog.LogLevel level)
        {
            return @"akka {
                      stdout-loglevel = " + level.Name + @"
                      loglevel = " + level.Name + @"
                      loggers=[""Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog""]
                      log-dead-letters-during-shutdown = off
                      log-config-on-start = on
                      actor {
                          debug {
                              receive = on
                              autoreceive = on
                              lifecycle = on
                              event-stream = on
                              unhandled = on
                              }
                          }";
        }
    }
}
