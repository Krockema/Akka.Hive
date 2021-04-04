using System;
using System.Collections.Generic;
using Akka.Hive.Action;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using Akka.Hive.Examples.Resources.Distributor;
using Akka.Hive.Logging;
using LogLevel = NLog.LogLevel;
using MachineAgent = Akka.Hive.Examples.Resources.Machine.MachineAgent;

namespace Akka.Hive.Examples
{
    class Program
    {
        public static void Main(string[] args)
        {
            Run();
        }
        private static void Run()
        {
            Console.WriteLine("Simulation world of Akka!");
           
            RunACore();

            Console.ReadLine();
        }

        private static void RunACore()
        {
            // LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_ACTORS, LogLevel.Info, LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AKKA, LogLevel.Warn);
            // LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace);
            //InternalLogger.LogToConsole = true;
            //InternalLogger.LogLevel = LogLevel.Trace;
            var time = new Time(new DateTime(2000,01,01));
            HiveConfig engineConfig =  CreateSimulationApproach(time); 
                                       //CreateHolonicApproach(time);
            // to Swap to Simulation use  CreateSimulationApproach(time);

            Hive engine = new Hive(engineConfig);
            var r = new Random();

            var jobDistributor =
                engine.ActorSystem.ActorOf(JobDistributor.Props(engine.ActorSystem.EventStream
                                                                    , engine.ContextManager
                                                                    , time
                                                                    , engineConfig),
                    "JobDistributor");

            // Tell all Machines
            for (int i = 0; i < 3; i++)
            {
                // Create a message
                var createMachines = new JobDistributor.AddMachine(null, jobDistributor);
                engine.ContextManager.Tell(createMachines, null);
            }

            // example to monitor for FinishWork Messages.
            var monitor = engine.ActorSystem.ActorOf(props: Monitoring.WorkTimeMonitor.Props(time: time),
                name: "Monitor");

            Console.ReadKey();
            Console.WriteLine("Systen is running!");

            for (int i = 0; i < 300; i++)
            {
                var materialRequest = new MaterialRequest(CreateBOM(), new Dictionary<int, bool>(), 0, r.Next(50, 500), true);
                var request = new JobDistributor.ProductionOrder(materialRequest, jobDistributor);
                engine.ContextManager.Tell(request, null);
            }

            if (engine.IsReady())
            {
                var terminated = engine.RunAsync();
                new StateManager().Continuation(engineConfig.Inbox, engine);
                terminated.Wait();
            }

            Console.WriteLine("Systen is shutdown!");
            Console.WriteLine("System Runtime " + engine.ActorSystem.Uptime);
        }

        private static HiveConfig CreateHolonicApproach(Time time)
        {
            var ActionFactory = new ActionFactory(agent =>
            {
                return agent switch
                {
                    MachineAgent => new MachineHolon(agent),
                    JobDistributor => new JobDistributorHolon(agent),
                    _ => throw new Exception($"Could not match agent type! Type was { agent.GetType().Name }")
                };
            });
            
            return HiveConfig.CreateHolonConfig(debugAkka: false
                , debugHive: true
                , interruptInterval: TimeSpan.FromSeconds(10)
                , startTime: time
                , actorActionFactory: ActionFactory);
        }

        private static HiveConfig CreateSimulationApproach(Time time)
        {
            return HiveConfig.CreateSimulationConfig(debugAkka: false
                , debugHive: true
                , interruptInterval: TimeSpan.FromMinutes(10)
                , startTime: time
                , timeToAdvance: TimeSpan.FromSeconds(0));
        }
        


        public static Material CreateBOM()
        {
            return new Material
            {
                Id = 1
                , Name = "Table"
                , AssemblyDuration = 5
                , Quantity = 1
                , Materials = new List<Material> { new Material { Id = 2
                                                                , Name = "Leg"
                                                                , AssemblyDuration = 3
                                                                , ParrentMaterialID = 1
                                                                , Quantity = 4
                                                                , IsReady = true } }
            };
        }
    }
}
