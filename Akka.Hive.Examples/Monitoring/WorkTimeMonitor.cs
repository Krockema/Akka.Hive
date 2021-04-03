using Akka.Actor;
using System;
using System.Collections.Generic;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using MachineAgent = Akka.Hive.Examples.Resources.Machine.MachineAgent;

namespace Akka.Hive.Examples.Monitoring
{
    public class WorkTimeMonitor : MessageMonitor
    {
        public WorkTimeMonitor(Time time) 
            : base(time, new List<Type> { typeof(MachineAgent.FinishWork) })
        {
        }

        protected override void EventHandle(object o)
        {
            // base.EventHandle(o);
            var m = o as MachineAgent.FinishWork;
            var material = m.Message as MaterialRequest;
            Console.WriteLine("Finished: " + material.Material.Name + " on Time: " + Time.Value);
        }

        public static Props Props(Time time)
        {
            return Akka.Actor.Props.Create(() => new WorkTimeMonitor(time));
        }
    }
}
