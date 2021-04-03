using System;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Action
{
    public abstract class Holon : IHiveAction
    {
        /// <summary>
        /// Holds private ref to the subordinate actor.
        /// </summary>
        private HiveActor Actor { get; }

        /// <summary>
        /// Creates an holon for a subordinate HiveActor
        /// </summary>
        /// <param name="actor">subordinate HiveActor</param>
        protected Holon(HiveActor actor)
        {
            Actor = actor;
        }

        /// <summary>
        /// Send call implementation for normal Time
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="waitFor">optional: possible delay until the message shall be send</param>
        public virtual void Send(IHiveMessage message, TimeSpan waitFor = new TimeSpan())
        {
            if (waitFor.Equals(TimeSpan.Zero))
            {
                message.Target.Tell(message: message, sender: Actor.Self);
            }
            else
            {
                Schedule(waitFor, message);
            }
        }

        /// <summary>
        /// Schedule call implementation for normal Time
        /// </summary>
        /// <param name="delay">delay until the message shall be send</param>
        /// <param name="message">message to send</param>
        public virtual void Schedule(TimeSpan delay, IHiveMessage message)
        {
            var atTime = Actor.Time.Add(delay);
            var s = new HiveMessage.Schedule(atTime, message);
            Actor.Timers.StartSingleTimer(Guid.NewGuid(), s, delay);
        }

        /// <summary>
        /// Throws in Default due to bad practice, message should be scheduled on source
        /// ScheduleMessage call implementation for normal time
        /// </summary>
        /// <param name="atTime"></param>
        /// <param name="message"></param>
        public virtual void ScheduleMessages(Time atTime, HiveMessage message)
        {
            throw new Exception("Should not be used for Holonic approaches.");
        }

        /// <summary>
        /// Call Finish implementation calls subordinate actor finish.
        /// </summary>
        /// <param name="finish"></param>
        public virtual void Finish(IHiveMessage finish)
        {
            Actor.Finish();
        }

        /// <summary>
        /// Possible Action implementation that is executed before Start
        /// </summary>
        public abstract void PreStart();

        /// <summary>
        /// Possible Action implementation that is executed After Stop
        /// </summary>
        public abstract void PostStop();
    }
}