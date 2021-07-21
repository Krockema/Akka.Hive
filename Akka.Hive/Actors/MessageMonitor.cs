using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Logging;
using NLog;
using static Akka.Hive.Definitions.HiveMessage;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// ... is a  Message interceptor that is listening to the void for a given set of object types
    /// </summary>
    public abstract class MessageMonitor : UntypedActor, ILogReceive
    {
        protected Time Time;
        private readonly List<Type> _channels;
        private readonly NLog.Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
       
        public MessageMonitor(Time time, List<Type> channels)
        {
            Time = time;
            _channels = channels;
                       
        }

        protected override void PreStart()
        {
            _channels.ForEach(channel => Context.System.EventStream.Subscribe(Self, channel));
            Context.System.EventStream.Subscribe(Self, typeof(AdvanceTo));

            base.PreStart();
        }

        protected override void PostStop()
        {
            _channels.ForEach(channel => Context.System.EventStream.Unsubscribe(Self, channel));
            Context.System.EventStream.Unsubscribe(Self, typeof(AdvanceTo));
            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case AdvanceTo m: Time = m.Time;
                    break;
                case Shutdown c: Shutdown();
                    break;
                default:
                    EventHandle(message);
                    break;
            }
        }
        
        protected virtual void EventHandle(object o)
        {
            //Unhandled(o);
            _logger.Error($"Letter captured: { o.ToString() }, sender: { Sender }");
        }

        protected virtual void Shutdown()
        {
            Context.Stop(Self);
        }
    }
}
