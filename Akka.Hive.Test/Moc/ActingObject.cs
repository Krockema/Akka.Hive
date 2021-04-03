using System;
using Akka.Actor;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;

namespace Akka.Hive.Test.Moc
{
    public class ActingObject : HiveActor
    {
        public ActingObject(IActorRef simulationContext, Time time, HiveConfig engineConfig) : base(simulationContext, time, engineConfig)
        {
        }

        protected override void Do(object process)
        {
            switch (process)
            {
                case Work w: Schedule((TimeSpan)w.Message, new Work("Done", Sender));
                    break;
                default:
                    throw new Exception("No Message Handler implemented for " + process.GetType().Name);
            }
        }
    }
}
