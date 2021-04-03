using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Test
{
    public class MessageFactory
    {
        public static HiveMessage Create(IActorRef target, string source, object obj)
        {
            return new HiveMessage (obj, target);
        }
    }
}