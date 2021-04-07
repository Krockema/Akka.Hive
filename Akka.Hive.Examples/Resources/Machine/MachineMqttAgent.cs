using System;
using Akka.Actor;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;
using Akka.Hive.Examples.Resources.Distributor;
using Akka.Hive.Interfaces;
using NLog;

namespace Akka.Hive.Examples.Resources.Machine
{
    partial class MachineMqttAgent : HiveActor, IWithExternalConnection
    {
        // Temp for test
        Random r = new Random(1337);
        private MaterialRequest _workingOn;
        private readonly IActorRef _jobDistributor;

        public Action<object> SendExtern { get; set; }

        public MachineMqttAgent(IActorRef context, Time time, HiveConfig engineConfig, IActorRef jobDistributor) : base(context, time, engineConfig)
        {
            Logger.Log(LogLevel.Info, "Time: " + Time.Value + " - " + Self.Path + " is Ready");
            _jobDistributor = jobDistributor;
        }
        public static Props Props(IActorRef simulationContext, Time time, HiveConfig engineConfig, IActorRef jobDistributor)
        {
            return Actor.Props.Create(() => new MachineMqttAgent(simulationContext, time, engineConfig, jobDistributor));
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case MachineAgent.Work m: DoWork(m); break;
                case MachineAgent.FinishWork f: WorkDone(f); break;
                case MachineAgent.MachineReady r: SetMachineReady(r); break;
                default: new Exception("Message type could not be handled by SimulationElement"); break;
            }
        }

        internal void DoWork(MachineAgent.Work m)
        {
            _workingOn = m.Message as MaterialRequest;
            SendExtern(new [] { _workingOn.Material.Name, _workingOn.Material.AssemblyDuration.ToString() });
            Logger.Log(LogLevel.Info, "{0} {1} started to work : {2} ",  new object[] {  this.Time.Value, Self.Path.Name, _workingOn.Material.Name });
            
        }

        private void WorkDone(MachineAgent.FinishWork f)
        {
            Logger.Log(LogLevel.Info, "{0} {1} finished to work : {2} ",  new object[] { this.Time.Value, Self.Path.Name, _workingOn.Material.Name });
            Send(new JobDistributor.ProductionOrderFinished(_workingOn, Context.Parent));
            _workingOn = null;
        }
        private void SetMachineReady(MachineAgent.MachineReady msg)
        {
            Console.WriteLine(msg);
            Send(new MachineAgent.MachineReady(msg.Message, _jobDistributor));
        }

        protected override void Finish()
        {
            base.Finish();
        }
    }
}