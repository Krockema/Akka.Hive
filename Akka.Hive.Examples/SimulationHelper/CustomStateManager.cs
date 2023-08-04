using Akka.Actor;
using Akka.Hive.Actors;

namespace Akka.Hive.Examples.SimulationHelper
{
    public class CustomStateManager : StateManager
    {
        private IActorRef _distributorRef;

        public static Props Props(Hive hive, IActorRef dist)
        {
            var props = Actor.Props.Create(() => new CustomStateManager(hive, dist));
            return props;
        }


        public CustomStateManager(Hive hive, IActorRef distributorRef) : base(hive)
        {
            _distributorRef = distributorRef;
        }
        
        public override void AfterSimulationStopped()
        {
            var statRequest = new RequestStatistics(_distributorRef);
            _distributorRef.Ask(statRequest).Wait();
        }
    }
}
