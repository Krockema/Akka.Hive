using System;
using System.Collections.Generic;
using Akka.Hive.Action;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using Akka.Hive.Examples.Resources;
using Akka.Hive.Examples.Resources.Distributor;
using Akka.Hive.Examples.Resources.Machine;
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
            Console.WriteLine("Welcome to the World of Akka.Hive!");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("What mode do you want to start?");
            Console.WriteLine(" ");
            Console.WriteLine(" (1) Simulation with virtual clock");
            Console.WriteLine(" (2) Simulation real time clock");
            Console.WriteLine(" (3) Real time clock with Mqtt Endpoint ");

            var inputValid = false;
            while (!inputValid)
            {
                var input = Console.ReadLine();
                var inputParsed = int.Parse(input);

                inputValid = (1 <= inputParsed && inputParsed <= 3);
                switch (inputParsed)
                {
                    case 1: RunHive(CreateSimulationApproach(new Time(new DateTime(2000,01,01)))); break;
                    case 2: RunHive(CreateHolonicApproach(Time.Now)); break;
                    case 3: RunHive(CreateHolonicApproach(Time.Now), true); break;
                    default: Console.WriteLine("Could not parse input. Try again."); break;
                }
                
            }
        }

        private static void RunHive(HiveConfig hiveConfig, bool withMqtt = false)
        {
            // LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AGENTS, LogLevel.Info, LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_ACTORS, LogLevel.Info, LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AKKA, LogLevel.Warn);
            // LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace);
            //InternalLogger.LogToConsole = true;
            //InternalLogger.LogLevel = LogLevel.Trace;
            var time = hiveConfig.StartTime;


            Hive hive = new Hive(hiveConfig);
            var r = new Random();

            var jobDistributor =
                hive.ActorSystem.ActorOf(props: JobDistributor.Props(hive.ActorSystem.EventStream
                                                                    , hive.ContextManager
                                                                    , time
                                                                    , hiveConfig),
                                         name: "JobDistributor");

            // Tell all Machines
            for (int i = 0; i < 3; i++)
            {
                // Create Machine Actors
                var reg = (withMqtt) ? MachineRegistration.CreateMqtt(hive.ContextManager, time, hiveConfig, jobDistributor):
                          /* else */   MachineRegistration.CreateDefault(hive.ContextManager, time, hiveConfig);
                
                var createMachines = new JobDistributor.AddMachine(reg, jobDistributor);
                hive.ContextManager.Tell(createMachines, null);
            }

            // example to monitor for FinishWork Messages.
            var monitor = hive.ActorSystem.ActorOf(props: Monitoring.WorkTimeMonitor.Props(time: time),
                name: "Monitor");
            Console.WriteLine("Machines initialized. Press any key to continue.");
            Console.ReadKey();
            Console.WriteLine("System is running!");

            for (int i = 0; i < 5; i++)
            {
                var materialRequest = new MaterialRequest(MaterialFactory.CreateTable(), new Dictionary<int, bool>(), 0, r.Next(50, 500), true);
                var request = new JobDistributor.ProductionOrder(materialRequest, jobDistributor);
                hive.ContextManager.Tell(request, null);
            }

            Console.WriteLine("Orders Distributed. Press any key to continue.");
            if (hive.IsReady())
            {
                var terminated = hive.RunAsync();
                new StateManager().Continuation(hiveConfig.Inbox, hive);
                terminated.Wait();
            }

            Console.WriteLine("Systen is shutdown!");
            Console.WriteLine("System Runtime " + hive.ActorSystem.Uptime);
        }

        private static HiveConfig CreateHolonicApproach(Time time)
        {
            var ActionFactory = new ActionFactory(agent =>
            {
                return agent switch
                {
                    MachineMqttAgent => new MachineMqttActions(agent),
                    MachineAgent => new HolonActions(agent),
                    JobDistributor => new HolonActions(agent),
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
    }
}
