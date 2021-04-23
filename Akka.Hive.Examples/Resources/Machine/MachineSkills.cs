using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Examples.Resources.Machine
{
    partial class MachineAgent
    {
        public enum Command
        {
            Ready
        }

        public record Work : HiveMessage
        {
            public Work(object message, IActorRef target) : base(message, target)
            { }
        }
    
        
        public record FinishWork : HiveMessage
        {
            public FinishWork(object Message, IActorRef target) : base(Message, target)
            { }
        }
        public record MachineReady : HiveMessage
        {
            public MachineReady(object Message, IActorRef target) : base(Message, target)
            { }
        }
    }
}
