using Akka.Hive.Action;
using Akka.Hive.Actors;

namespace Akka.Hive.Examples.Domain
{
    public class JobDistributorHolon : Holon
    {
        public JobDistributorHolon(HiveActor actor) : base(actor)
        {
        }

        public override void PreStart()
        {
            // not required
        }

        public override void PostStop()
        {
            // not required
        }
    }
}