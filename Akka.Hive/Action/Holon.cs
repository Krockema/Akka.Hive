using System;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Action
{
    public abstract class Holon : IHiveAction
    {
        /// <summary>
        /// Holds internal Actor Ref.
        /// </summary>
        protected HiveActor ActorElement { get; }
        protected Holon(HiveActor actor)
        {
            ActorElement = actor;
        }

        public virtual void Send(IHiveMessage instruction, TimeSpan waitFor = new TimeSpan())
        {
            instruction.Target.Tell(message: instruction, sender: ActorElement.Self);
        }

        public virtual void Schedule(TimeSpan delay, IHiveMessage message)
        {
            var atTime = ActorElement.Time.Add(delay);
            var s = new HiveMessage.Schedule(atTime, message);
            ActorElement.Timers.StartSingleTimer(Guid.NewGuid(), s, delay);
        }

        public virtual void ScheduleMessages(Time atTime, HiveMessage message)
        {
            Send(message);
        }

        public virtual void Finish(IHiveMessage finish)
        {
            ActorElement.Finish();
        }

        public abstract void PreStart();
        public abstract void PostStop();
    }
}