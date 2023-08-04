using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Akka.Hive.Action;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using Akka.Hive.Examples.Resources;
using Akka.Hive.Examples.Resources.Distributor;
using Akka.Hive.Examples.Resources.Machine;
using Akka.Hive.Examples.SimulationHelper;
using Akka.Hive.Logging;
using NLog;
using NLog.Common;
using static Akka.Hive.Definitions.HiveMessage;
using static Akka.Hive.Examples.Resources.Distributor.JobDistributor;
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
            Console.WriteLine(" (2) Simulation with sequencial virtual clock");
            Console.WriteLine(" (3) Simulation real time clock");
            Console.WriteLine(" (4) Real time clock with Mqtt Endpoint ");

            var inputValid = false;
            while (!inputValid)
            {
                var input = Console.ReadLine();
                var inputParsed = int.Parse(input);

                inputValid = (1 <= inputParsed && inputParsed <= 3);
                switch (inputParsed)
                {
                    case 1: RunHive(CreateSimulationApproach(new Time(new DateTime(2000,01,01)))); break;
                    case 2: RunHive(CreateSimulationApproach(new Time(new DateTime(2000, 01, 01)), sequencial: true)); break;
                    case 3: RunHive(CreateHolonicApproach(Time.Now)); break;
                    case 4: RunHive(CreateHolonicApproach(Time.Now), true); break;
                    default: Console.WriteLine("Could not parse input. Try again."); break;
                }
                
            }
        }

        private static IHiveConfig CreateHolonicApproach(Time time)
        {
            var tracer = new MessageTrace().AddTrace(typeof(MachineAgent), typeof(ProductionOrderFinished));
            return HiveConfig.ConfigureHolon()
                             .WithActionFactory(new ActionFactory(agent => {
                                 return agent switch
                                 {
                                     MachineMqttAgent => new MachineMqttActions(agent),
                                     MachineAgent => new HolonActions(agent),
                                     JobDistributor => new HolonActions(agent),
                                     _ => throw new Exception($"Could not match agent type! Type was { agent.GetType().Name }")
                                 };
                             })
                             )
                             .WithStateManagerProbs((hive, args) => CustomStateManager.Props(hive, (IActorRef)args[0]))
                             .WithDebugging(akka: false, hive: true)
                             .WithInterruptInterval(TimeSpan.FromSeconds(10))
                             .WithStartTime(time)
                             .WithMessageTracer(tracer)
                             .Build();
        }

        private static IHiveConfig CreateSimulationApproach(Time time, bool sequencial = false)
        {
            var tracer = new MessageTrace().AddTrace(typeof(MachineAgent), typeof(ProductionOrderFinished));
            return HiveConfig.ConfigureSimulation(sequencial)
            .WithTimeSpanToTerminate(TimeSpan.FromDays(365))
                             .WithDebugging(akka: false, hive: false)
                             .WithStateManagerProbs((hive, args) => CustomStateManager.Props(hive, (IActorRef)args[0]))
                             .WithInterruptInterval(TimeSpan.FromMinutes(480))
                             .WithStartTime(time)
                             .WithTickSpeed(TimeSpan.FromMilliseconds(0))
                             .WithMessageTracer(tracer)
                             .Build();
        }

        private static void RunHive(IHiveConfig hiveConfig, bool withMqtt = false)
        {
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_ACTORS, NLog.LogLevel.Info, NLog.LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_ACTORS, NLog.LogLevel.Warn, NLog.LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AKKA, NLog.LogLevel.Trace, NLog.LogLevel.Warn);
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AKKA, NLog.LogLevel.Info, NLog.LogLevel.Info);
            InternalLogger.LogToConsole = true;
            InternalLogger.LogLevel = NLog.LogLevel.Trace;
            var time = hiveConfig.StartTime;


            Hive hive = new (hiveConfig);
            var r = new Random();

            var jobDistributor =
                hive.ActorSystem.ActorOf(props: JobDistributor.Props(hive.ActorSystem.EventStream
                                                                    , hive.ContextManagerRef
                                                                    , time
                                                                    , hiveConfig),
                                         name: "JobDistributor");
            hive.InitStateManager(new[] { jobDistributor });
            // Tell all Machines
            for (int i = 0; i < 5; i++)
            {
                // Create Machine Actors
                var reg = (withMqtt) ? MachineRegistration.CreateMqtt(hive.ContextManagerRef, time, hiveConfig, jobDistributor):
                          /* else */   MachineRegistration.CreateDefault(hive.ContextManagerRef, time, hiveConfig);
                
                var createMachines = new JobDistributor.AddMachine(reg, jobDistributor);
                jobDistributor.Tell(createMachines, null);
            }

            
            //dead Letter Monitoring
            var monitorActor = hive.ActorSystem.ActorOf<DeadLetterMonitor>();
            // Subscribe to messages of type AllDeadLetters
            hive.ActorSystem.EventStream.Subscribe(monitorActor, typeof(AllDeadLetters));

            // example to monitor for FinishWork Messages.
            // var monitor = hive.ActorSystem.ActorOf(props: Monitoring.WorkTimeMonitor.Props(time: time, hiveConfig.MessageTrace.GetTracedMessages(typeof(MachineAgent)))
            //                                      , name: "Monitor");
            Console.WriteLine("Machines initialized. Press any key to continue.");
            Console.WriteLine("How many Tables shall be produced?");
            var noOfJobs = int.Parse(Console.ReadLine());
            Console.WriteLine("Got " + noOfJobs);
            Console.WriteLine("System is Starting!");

            for (int i = 0; i < noOfJobs; i++)
            {
                var materialRequest = new MaterialRequest(MaterialFactory.CreateTable(), new Dictionary<int, bool>(), 0, r.Next(0, noOfJobs*10), true);
                var request = new JobDistributor.ProductionOrder(materialRequest, jobDistributor);
                hive.ContextManagerRef.Tell(new Schedule(time.Add(TimeSpan.FromMinutes(1)), request), null);
            }

            Console.WriteLine("Orders Distributed. Press any key to continue.");
            Console.ReadKey();
            if (hive.IsReady())
            {
                
                var terminated = hive.RunAsync();
                terminated.Wait();
            }

            Console.WriteLine("Systen is shutdown!");
            Console.WriteLine("System Runtime " + hive.ActorSystem.Uptime);
        }

    }
}
