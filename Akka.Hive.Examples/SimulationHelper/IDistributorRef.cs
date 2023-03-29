using Akka.Actor;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Examples.SimulationHelper
{
    public interface IWithDistributorRef 
    {
        IStateManagerBase WithDistributor(IActorRef actorRef);
    }
}
