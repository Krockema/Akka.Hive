﻿using System;
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
        
        public Simulation(HiveActor actor)
        {
            _actor = actor;
        }

        public void ScheduleMessages(Time atTime, HiveMessage message)
        {
            if (!_messageStash.TryGetValue(atTime.Value, out PriorityQueue<HiveMessage> stash))
            {
                stash = new PriorityQueue<HiveMessage>();
                _messageStash.Add(atTime.Value, stash);
            }
            stash.Enqueue(message);
        }

        public void Send(IHiveMessage instruction, TimeSpan waitFor = new ())
        {
            if (waitFor.Equals(TimeSpan.Zero))
            {
                // instruction.Target.Tell(message: instruction, sender: Self);
                _actor.ContextManager.Tell(message: instruction, sender: _actor.Self);
            }
            else
            {
                // Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds((double)waitFor), Self, instruction, Self);
                Schedule(delay: waitFor, message: instruction);
            }
        }

        public void Schedule(TimeSpan delay, IHiveMessage message)
        {
            var atTime = _actor.Time.Add(delay);
            var s = new HiveMessage.Schedule(atTime, message);
            _actor.ContextManager.Tell(s, _actor.Self);
        }
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

        public void MapMessageToMethod(object message)
        {
            IHiveMessage m = message as IHiveMessage;
            _actor.Do(message);
            _actor.ContextManager.Tell(new HiveMessage.Done(m), ActorRefs.NoSender);
        }

        public void AdvanceTo(Time time) 
        {
            _actor.Time = time;
            _actor.PostAdvance();
            ReleaseMessagesForThisTime();
        }

        public void Finish(IHiveMessage finish)
        {
            _actor.Finish();
            _actor.ContextManager.Tell(new HiveMessage.Done(finish), ActorRefs.NoSender);
            
        }

        public void PreStart()
        {
            _actor.ActorContext.System.EventStream.Subscribe(_actor.Self, typeof(HiveMessage.AdvanceTo));
        }

        public void PostStop()
        {
            _actor.ActorContext.System.EventStream.Unsubscribe(_actor.Self, typeof(HiveMessage.AdvanceTo));
            var p = _actor.ActorContext.Parent;
            if (!(p == _actor.ContextManager))
                _actor.ContextManager.Tell(new HiveMessage.Finish(p), _actor.Self);
        }
    }
}