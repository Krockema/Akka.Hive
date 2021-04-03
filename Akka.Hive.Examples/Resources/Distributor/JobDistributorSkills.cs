using Akka.Actor;
using Akka.Hive.Definitions;

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
            public AddMachine(object message, IActorRef target) : base(message, target)
            {
            }
        }
    }
}
