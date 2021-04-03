using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Hive.Actors;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Action
{
    public class Simulation : IHiveAction
    {
        /// <summary>
        /// Holds internal Actor Ref.
        /// </summary>
        private readonly HiveActor _actor;
        /// <summary>
        /// Store for featured messages
        /// </summary>
        private readonly Dictionary<DateTime, PriorityQueue<HiveMessage>> _messageStash = new();
        /// <summary>
        /// Creates an Simulation action object for a subordinate HiveActor
        /// </summary>
        /// <param name="actor">subordinate HiveActor</param>
        public Simulation(HiveActor actor)
        {
            _actor = actor;
        }

        /// <summary>
        /// Schedules a Message for later occurrence on this actor
        /// </summary>
        /// <param name="atTime">Time of occurrence</param>
        /// <param name="message">Message to schedule</param>
        public void ScheduleMessages(Time atTime, HiveMessage message)
        {
            if (!_messageStash.TryGetValue(atTime.Value, out PriorityQueue<HiveMessage> stash))
            {
                stash = new PriorityQueue<HiveMessage>();
                _messageStash.Add(atTime.Value, stash);
            }
            stash.Enqueue(message);
        }

        /// <summary>
        /// Send a Message implementation for Virtual time
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="waitFor">optional: possible delay until the message shall be send</param>
        public void Send(IHiveMessage message, TimeSpan waitFor = new ())
        {
            if (waitFor.Equals(TimeSpan.Zero))
            {
                // instruction.Target.Tell(message: instruction, sender: Self);
                _actor.ContextManager.Tell(message: message, sender: _actor.Self);
            }
            else
            {
                // Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds((double)waitFor), Self, instruction, Self);
                Schedule(delay: waitFor, message: message);
            }
        }

        /// <summary>
        /// Schedule a message at target actor
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="message"></param>
        public void Schedule(TimeSpan delay, IHiveMessage message)
        {
            var atTime = _actor.Time.Add(delay);
            var s = new HiveMessage.Schedule(atTime, message);
            _actor.ContextManager.Tell(s, _actor.Self);
        }

        /// <summary>
        /// Releases all messages from store for the current time
        /// </summary>
        private void ReleaseMessagesForThisTime()
        {
            var thereWasWork = _messageStash.TryGetValue(_actor.Time.Value, out PriorityQueue<HiveMessage> stash);
            // One by one.
            while (stash != null && stash.Count() != 0)
                MapMessageToMethod(stash.Dequeue());
            // Free up Space
            if (thereWasWork)
                _messageStash.Remove(_actor.Time.Value);
        }

        /// <summary>
        /// Maps a specific message to the "Do" method of the Actor and sends "Done" to the supervising actor
        /// </summary>
        /// <param name="message"></param>
        public void MapMessageToMethod(object message)
        {
            var m = message as IHiveMessage;
            _actor.Do(message);
            _actor.ContextManager.Tell(new HiveMessage.Done(m), ActorRefs.NoSender);
        }

        /// <summary>
        /// Advances the local actor time to the given time
        /// </summary>
        /// <param name="time">new current time</param>
        public void AdvanceTo(Time time) 
        {
            _actor.Time = time;
            _actor.PostAdvance();
            ReleaseMessagesForThisTime();
        }

        /// <summary>
        /// Finished the current Agent and sends "Done" to the supervising actor
        /// </summary>
        /// <param name="finish"></param>
        public void Finish(IHiveMessage finish)
        {
            _actor.Finish();
            _actor.ContextManager.Tell(new HiveMessage.Done(finish), ActorRefs.NoSender);
            
        }

        /// <summary>
        /// Subscribes the Actor to the global clock to receive AdvanceTo messages
        /// </summary>
        public void PreStart()
        {
            _actor.ActorContext.System.EventStream.Subscribe(_actor.Self, typeof(HiveMessage.AdvanceTo));
        }

        /// <summary>
        /// Unsubscribes the Actor from the global clock
        /// </summary>
        public void PostStop()
        {
            _actor.ActorContext.System.EventStream.Unsubscribe(_actor.Self, typeof(HiveMessage.AdvanceTo));
            var p = _actor.ActorContext.Parent;
            if (!(p == _actor.ContextManager))
                _actor.ContextManager.Tell(new HiveMessage.Finish(p), _actor.Self);
        }
    }
}