﻿using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Hive.Action;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using static Akka.Hive.Definitions.HiveMessage;

namespace Akka.Hive
{
    public class Hive
    {
        public const string ContextName = "HiveContext";
        public ActorSystem ActorSystem { get; }
        public HiveConfig Config { get; }
        public IActorRef ContextManager { get; }
        /// <summary>
        /// Prepare Simulation Environment
        /// </summary>
        /// <param name="engineConfig">Several Simulation Configurations</param>
        public Hive(HiveConfig engineConfig)
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
            engineConfig.Inbox = Inbox.Create(ActorSystem);
            ContextManager = CreateContextRef(engineConfig);

        }

        private IActorRef CreateContextRef(HiveConfig engineConfig)
        {
            return engineConfig.ActorActionFactory.ActorActions switch
            {
                Actions.Simulation => ActorSystem.ActorOf(Props.Create(() 
                                   => new SimulationManager(engineConfig)), ContextName),
                Actions.Holon =>  ActorSystem.ActorOf(Props.Create(() 
                              => new HolonManager(engineConfig)), ContextName),
                _ => throw new NotImplementedException(),
            };
        }
        

        public bool IsReady()
        {
            var r = ContextManager.Ask(Command.IsReady).Result;
            return r is Command.IsReady;
        }

        public void Continue() {
            ContextManager.Tell(Command.Start);
        }


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
