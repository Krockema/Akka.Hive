using Akka.Hive.Action;
using Akka.Hive.Actors;

namespace Akka.Hive.Examples.Domain
{
    public class MachineHolon : Holon
    {
        public MachineHolon(HiveActor actor) : base(actor)
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