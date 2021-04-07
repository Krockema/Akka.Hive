using Akka.Hive.Action;
using Akka.Hive.Actors;

namespace Akka.Hive.Examples.Resources
{
    public class HolonActions : Holon
    {
        public HolonActions(HiveActor actor) : base(actor)
        {
        }

        public override void PreStart()
        {
            // not Required yet
        }

        public override void PostStop()
        {
            // not Required yet
        }
    }
}