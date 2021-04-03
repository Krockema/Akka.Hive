using System;
using Akka.Actor;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using Akka.Hive.Examples.Resources.Distributor;
using NLog;

namespace Akka.Hive.Examples.Resources.Machine
{
    partial class MachineAgent : HiveActor
    {
        // Temp for test
        Random r = new Random(1337);

        public MachineAgent(IActorRef simulationContext, Time time, HiveConfig engineConfig) : base(simulationContext, time, engineConfig)
        {
            Logger.Log(LogLevel.Info, "Time: " + Time.Value + " - " + Self.Path + " is Ready");
        }
        public static Props Props(IActorRef simulationContext, Time time, HiveConfig engineConfig)
        {
            return Akka.Actor.Props.Create(() => new MachineAgent(simulationContext, time, engineConfig));
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case Work m: DoWork(m); break;
                case FinishWork f: WorkDone(f); break;
                default: new Exception("Message type could not be handled by SimulationElement"); break;
            }
        }

        private void DoWork(MachineAgent.Work m)
        {
            var material = m.Message as MaterialRequest;
            var dur = material.Material.AssemblyDuration + r.Next(-1, 2);
            Schedule(TimeSpan.FromSeconds(dur),  new FinishWork(m.Message, Self));
            Logger.Log(LogLevel.Info, "{0} {1} started to work : {2} ",  new object[] {  this.Time.Value, Self.Path.Name, material.Material.Name });
        }

        private void WorkDone(FinishWork finishWork)
        {
            Logger.Log(LogLevel.Info, "{0} {1} finished to work : {2} ",  new object[] { this.Time.Value, Self.Path.Name, ((MaterialRequest)(finishWork.Message)).Material.Name });
            var material = finishWork.Message as MaterialRequest;
            Send(new JobDistributor.ProductionOrderFinished(material, Context.Parent));
        }

        protected override void Finish()
        {
            base.Finish();
        }


    }
}