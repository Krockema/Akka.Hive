using System;
using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// A Time Monitor that does perform an injected action on TimeAdvance event.
    /// </summary>
    public class TimeMonitor :  ReceiveActor
    {
        public TimeMonitor(Action<Time> report)
        {
            Receive<HiveMessage.AdvanceTo>(dl =>
                report(dl.Time)
            );
        }

        protected sealed override void PreStart()
        {
            Context.System.EventStream.Subscribe(Self, typeof(HiveMessage.AdvanceTo));
            base.PreStart();
        }

        protected sealed override void PostStop()
        {
            //_SimulationContext.Tell(Command.DeRegistration, Self);
            Context.System.EventStream.Unsubscribe(Self, typeof(HiveMessage.AdvanceTo));
            base.PostStop();
        }
    }
}

