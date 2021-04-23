using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Resources.Machine;

namespace Akka.Hive.Examples.Domain
{
    public class MachineRegistration 
    {
        public static MachineRegistration CreateDefault(IActorRef contextManager, Time time, HiveConfig hiveConfig)
        {
            var r = new MachineRegistration();
            r.IsReady = true;
            r.IsConnected = true;
            r.MachineProps = MachineAgent.Props(contextManager, time, hiveConfig);
            return r;
        }

        public static MachineRegistration CreateMqtt(IActorRef contextManager, Time time, HiveConfig hiveConfig, IActorRef jobDistributor)
        {
            var r = new MachineRegistration();
            r.IsReady = false;
            r.IsConnected = false;
            r.MachineProps = MachineMqttAgent.Props(contextManager, time, hiveConfig, jobDistributor);
            return r;
        }
        public bool IsReady { get; set; }
        public bool IsConnected { get; set; }
        public IActorRef ActorRef { get; set; }
        public Props MachineProps { get; set; }
        public MachineRegistration SetWorking() { 
            IsReady = false;
            return this;
        }

        public MachineRegistration SetReady()
        {
            IsReady = true;
            return this;
        }

        public MachineRegistration SetConnected()
        {
            IsConnected = true;
            return this;
        }
    }
}