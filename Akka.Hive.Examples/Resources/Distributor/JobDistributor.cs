using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using Akka.Hive.Examples.SimulationHelper;
using LogLevel = NLog.LogLevel;
using MachineAgent = Akka.Hive.Examples.Resources.Machine.MachineAgent;

namespace Akka.Hive.Examples.Resources.Distributor
{
    partial class JobDistributor : HiveActor
    {
        private int MaterialCounter = 0;
        public HashSet<MachineRegistration> Machines { get; set; } = new ();

        public PriorityQueue<MaterialRequest> ReadyItems { get; set; } = new ();

        public HashSet<MaterialRequest> WaitingItems { get; set; } = new ();

        public static Props Props(EventStream eventStream, IActorRef simulationContext, Time time, IHiveConfig engineConfig)
        {
            return Akka.Actor.Props.Create(() => new JobDistributor(simulationContext, time, engineConfig));
        }
        public JobDistributor(IActorRef contextManager, Time time, IHiveConfig engineConfig) 
            : base(contextManager, time, engineConfig)
        {
            
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case ProductionOrder m  : MaterialRequest(m); break;
                case Command.GetWork    : PushWork(); break;
                case AddMachine m       : CreateMachines(m.MachineRegistration, Machines.Count + 1, this.Time, this.EngineConfig); break; 
                case MachineAgent.MachineReady m : SetMachineReady(m); break;
                case ProductionOrderFinished m: ProvideMaterial(m); break;
                case RequestStatistics m: CreateStats(); break;
                default: _ = new Exception("Message type could not be handled by SimulationElement"); break;
            }
        }

        /// <summary>
        /// solve Tree
        /// </summary>
        /// <param name="request"></param>
        private void MaterialRequest(object o)
        {
            var p = o as ProductionOrder;
            var request = p?.Message as MaterialRequest;
            if (request?.Material.Materials != null)
            {
                foreach (var child in request.Material.Materials)
                {
                    for (int i = 0; i < child.Quantity; i++)
                    {
                        var childRequest = new MaterialRequest(material: child,
                                                          childRequests: null,
                                                                 parent: request.Id,
                                                                    due: request.Due - request.Material.AssemblyDuration - child.AssemblyDuration,
                                                                 isHead: false);
                        request.ChildRequests.Add(childRequest.Id, false);
                        var po = new ProductionOrder(childRequest, Self);
                        Send(po);
                    }
                }
            }
            if (request != null && request.Material.IsReady)
                ReadyItems.Enqueue(request);
            else
                WaitingItems.Add(request);

            PushWork();
        }

        private void PushWork()
        {
            if (Machines.Any(x => x.IsReady && x.IsConnected) && ReadyItems.Count() != 0)
            {
                var machine = Machines.First(X => X.IsReady);
                var m = new MachineAgent.Work(ReadyItems.Dequeue(), machine.ActorRef);
                machine.SetWorking();
                Send(m);
            };
        }

        private void CreateMachines(MachineRegistration registration, int machineNumber, Time time, IHiveConfig engineConfig)
        {
            Logger.Log(LogLevel.Warn, "Creating Machine No: {arg} !",  new object[] { machineNumber });
            registration.ActorRef = Context.ActorOf(registration.MachineProps, "Machine_" + machineNumber);
            Machines.Add(registration);
        }

        private void SetMachineReady(MachineAgent.MachineReady ready)
        {
            Machines.Single(x => x.ActorRef.Equals(Sender))
                    .SetReady()
                    .SetConnected();
            PushWork();
        }


        private void ProvideMaterial(object o)
        {
            var po = o as ProductionOrderFinished;
            var request = po?.Message as MaterialRequest;
            if (request?.Material.Name == "Table")
                Logger.Log(LogLevel.Warn, "Simulation: Table No: {arg} has finished at {}",  new object[] { ++MaterialCounter, Time.Value });
            //Console.WriteLine("Time: " + TimePeriod + " Number " + MaterialCounter + " Finished: " + request.Material.Name);
            if (!request.IsHead)
            {
                var parent = WaitingItems.Single(x => x.Id == request.Parent);
                parent.ChildRequests[request.Id] = true;
                
                // now check if item can be deployd to ReadyQueue
                if (parent.ChildRequests.All(x => x.Value))
                {
                    WaitingItems.Remove(parent);
                    ReadyItems.Enqueue(parent);
                }
            }
            Machines.Single(x => x.ActorRef.Equals(Sender))
                    .SetReady();
            PushWork();
        }

        private void CreateStats()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Production orders remaining " + ReadyItems.Count());
            Console.ResetColor();
            Sender.Tell("Done", Self);
        }
        protected override void Finish()
        {
            Logger.Log(LogLevel.Debug, "Simulation: {arg} has been Killed",  new object[] { Sender.Path });
        }
    }
}
