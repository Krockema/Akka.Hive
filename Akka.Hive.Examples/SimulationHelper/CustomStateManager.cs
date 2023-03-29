using Akka.Actor;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Examples.SimulationHelper
{
    public class CustomStateManager : StateManager, IWithDistributorRef
    {
        private CustomStateManager() : base() {

        }
        private IActorRef DistributorRef;

        public static IWithDistributorRef Base => new CustomStateManager();
        
        public IStateManagerBase WithDistributor(IActorRef actorRef)
        {
            DistributorRef = actorRef;
            return this;
        }
        
        public override void AfterSimulationStopped()
        {
            var statRequest = new RequestStatistics(DistributorRef);
            DistributorRef.Ask(statRequest).Wait();
        }
    }
}
