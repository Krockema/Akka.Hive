using System;
using Akka.Actor;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Action
{
    /// <summary>
    /// ... is a class that provides simulation functionality to the HiveActor.
    /// </summary>
    public class Sequencial : Simulation , IHiveAction
    {
        /// <summary>
        /// Creates an Simulation action object for a subordinate HiveActor
        /// </summary>
        /// <param name="actor">subordinate HiveActor</param>
        public Sequencial(HiveActor actor) : base(actor) { }

        /// <summary>
        /// Send a Message implementation for Virtual time
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="waitFor">optional: possible delay until the message shall be send</param>
        public override void Send(IHiveMessage message)
        {
            message = message.WithSender(_actor.Self);
            // instruction.Target.Tell(message: instruction, sender: Self);
            _actor.ContextManager.Tell(message: message, sender: _actor.Self);
        }

        /// <summary>
        /// Schedule a message at target actor
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="message"></param>
        public override void Schedule(TimeSpan delay, IHiveMessage message)
        {
            message = message.WithSender(_actor.Self);
            var atTime = _actor.Time.Add(delay);
            var s = new HiveMessage.Schedule(atTime, message);
            _actor.ContextManager.Tell(s, _actor.Self);
        }
    }
}