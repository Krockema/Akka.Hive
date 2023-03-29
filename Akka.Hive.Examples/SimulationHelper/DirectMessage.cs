using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Examples.SimulationHelper
{
    public record RequestStatistics : HiveMessage, IDirectMessage
    {
        public RequestStatistics(IActorRef target): base(target: target, message: null)
        {

        }
    }
}
