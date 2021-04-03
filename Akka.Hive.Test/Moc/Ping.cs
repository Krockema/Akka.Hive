using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Test.Moc
{
    public record Ping : HiveMessage
    {
        public const string Name = "PING";
        public Ping(object message, IActorRef target) : base(message, target)
        {

        }

    }
}
