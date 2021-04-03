using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using LogLevel = NLog.LogLevel;
using MachineAgent = Akka.Hive.Examples.Resources.Machine.MachineAgent;

namespace Akka.Hive.Examples.Resources.Distributor
{
    partial class JobDistributor : HiveActor
    {
        private int MaterialCounter = 0;
        public Dictionary<IActorRef, bool> Machines { get; set; } = new Dictionary<IActorRef, bool>();

        public PriorityQueue<MaterialRequest> ReadyItems { get; set; } = new PriorityQueue<MaterialRequest>();

        public HashSet<MaterialRequest> WaitingItems { get; set; } = new HashSet<MaterialRequest>();

        public static Props Props(EventStream eventStream, IActorRef simulationContext, Time time, HiveConfig engineConfig)
        {
            return Akka.Actor.Props.Create(() => new JobDistributor(simulationContext, time, engineConfig));
        }
        public JobDistributor(IActorRef contextManager, Time time, HiveConfig engineConfig) 
            : base(contextManager, time, engineConfig)
        {
            
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case ProductionOrder m  : MaterialRequest(m); break;
                case Command.GetWork    : PushWork(); break;
                case AddMachine m       : CreateMachines(Machines.Count + 1, this.Time, this.EngineConfig); break;
                case ProductionOrderFinished m: ProvideMaterial(m); break;
                default: new Exception("Message type could not be handled by SimulationElement"); break;
            }
        }

        /// <summary>
        /// solve Tree
        /// </summary>
        /// <param name="request"></param>
        private void MaterialRequest(object o)
        {
            var p = o as ProductionOrder;
            var request = p.Message as MaterialRequest;
            if (request.Material.Materials != null)
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
                        ContextManager.Tell(po, Self);
                    }
                }
            }
            if (request.Material.IsReady)
                ReadyItems.Enqueue(request);
             else
                WaitingItems.Add(request);

            PushWork();
        }

        private void PushWork()
        {
            if (Machines.ContainsValue(true) && ReadyItems.Count() != 0)
            {
                var key = Machines.First(X => X.Value == true).Key;
                Machines.Remove(key);
                var m = new MachineAgent.Work(ReadyItems.Dequeue(), key);
                Machines.Add(key, false);
                ContextManager.Tell(m, Sender);
            };
        }

        private void CreateMachines(int machineNumber, Time time, HiveConfig engineConfig)
        {
            Logger.Log(LogLevel.Warn, "Creating Maschine No: {arg} !",  new object[] { machineNumber });
            Machines.Add(Context.ActorOf(MachineAgent.Props(ContextManager, time, engineConfig), "Maschine_" + machineNumber), true);
        }

        private void ProvideMaterial(object o)
        {
            var po = o as JobDistributor.ProductionOrderFinished;
            var request = po.Message as MaterialRequest;
            if (request.Material.Name == "Table")
                Logger.Log(LogLevel.Warn, "Simulation: Table No: {arg} has finished at {}",  new object[] { ++MaterialCounter, Time.Value });
            //Console.WriteLine("Time: " + TimePeriod + " Number " + MaterialCounter + " Finished: " + request.Material.Name);
            if (!request.IsHead)
            {
                var parrent = WaitingItems.Single(x => x.Id == request.Parent);
                parrent.ChildRequests[request.Id] = true;
                
                // now check if item can be deployd to ReadyQueue
                if (parrent.ChildRequests.All(x => x.Value == true))
                {
                    WaitingItems.Remove(parrent);
                    ReadyItems.Enqueue(parrent);
                }
            }
            Machines.Remove(Sender);
            Machines.Add(Sender, true);

            
            PushWork();
        }

        protected override void Finish()
        {
            Logger.Log(LogLevel.Debug, "Simulation: {arg} has been Killed",  new object[] { Sender.Path });
        }
    }
}
