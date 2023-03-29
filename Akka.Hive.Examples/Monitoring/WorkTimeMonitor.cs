using Akka.Actor;
using System;
using System.Collections.Generic;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using MachineAgent = Akka.Hive.Examples.Resources.Machine.MachineAgent;
using Akka.Hive.Examples.Resources.Distributor;
using System.Diagnostics.Metrics;

namespace Akka.Hive.Examples.Monitoring
{
    public class WorkTimeMonitor : MessageMonitor
    {
        private int _pieces = 0; 
        public WorkTimeMonitor(Time time, List<Type> messageTypes) 
            : base(time, messageTypes)
        {
        }

        public static Props Props(Time time, List<Type> channels)
        {
            return Actor.Props.Create(() => new WorkTimeMonitor(time, channels));
        }

        protected override void EventHandle(object o)
        {
            // base.EventHandle(o);
            var m = o as JobDistributor.ProductionOrderFinished;
            var material = m.Message as MaterialRequest;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Finished: " + material.Material.Name + " on Time: " + material.Done);
            Console.ResetColor();
        }
    }
}
