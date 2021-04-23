using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Examples.Domain;

namespace Akka.Hive.Examples.Resources.Distributor
{
    partial class JobDistributor
    {
        public enum Command
        {
            GetWork
        }
        public record ProductionOrder : HiveMessage
        {
            public ProductionOrder(object message, IActorRef target) : base(message, target)
            {
            }
        }

        public record ProductionOrderFinished : HiveMessage
        {
            public ProductionOrderFinished(object message, IActorRef target) : base(message, target)
            {
            }
        }

        public record AddMachine : HiveMessage
        {
            public AddMachine(MachineRegistration message, IActorRef target) : base(message, target)
            {
            }
            public MachineRegistration MachineRegistration => base.Message as MachineRegistration;
        }
    }
}
