using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Test.Moc
{
    public record Work : HiveMessage
    {
        public Work(object message, IActorRef target) : base(message, target)
        {

        }
    }
}
