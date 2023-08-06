using System;
using Akka.Actor;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Definitions
{
    /// <summary>
    /// Data Structure 
    /// </summary>
    public record HiveMessage : IComparable<IHiveMessage>, IHiveMessage
    {
        /// <summary>
        /// Generic Message identifier
        /// </summary>
        public Guid Key { get; }
        /// <summary>
        /// !- Immutable -! Message Object
        /// May change to data type "record" in the future!
        /// </summary>
        public object Message { get; }
        /// <summary>
        /// Target Actor to whom the Simulation Message shall be forwarded.
        /// </summary>
        public IActorRef Target { get; }
        /// <summary>
        /// Target Actor to whom the Simulation Message shall be forwarded.
        /// </summary>
        public IActorRef Sender { get; init; }
        /// <summary>
        /// Priority to Order msg
        /// </summary>
        public Priority Priority { get; init; }
        /// <summary>
        /// Log this Message to the event Stream.
        /// </summary>
        public bool LogThis { get; init; }        

        /// <summary>
        /// Message that is send in simulation context when an message has been processed
        /// </summary>
        public record Done : HiveMessage
        {
            public Done(IHiveMessage with)
                : base(target: with.Target, message: with) { }
        }

        /// <summary>
        /// Field to fill if Broadcast is Required.
        /// </summary>
        public ActorSelection TargetSelection { get; }
        /// <summary>
        /// Comparer for Priority Queue
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IHiveMessage other)
        {
            if (this.Priority < other.Priority) return -1;
            if (this.Priority > other.Priority) return 1;
            return 0;
        }

        /// <summary>
        /// Agent Intern  finish current Actor and tell parents
        /// </summary>
        internal record Finish : HiveMessage
        {
            public Finish(IActorRef parent) 
                : base(target: parent, message: null) { }

        };

        /// <summary>
        /// Use this message to force System shutdown, it waits till all messages for the current timespan are processed
        /// and then terminates the System, regardless of feature messages.
        /// </summary>
        public record Shutdown : HiveMessage
        {
            public Shutdown(IActorRef simulationContextRef) : base(null, simulationContextRef) { }
        }

        /// <summary>
        /// Message to Advance the local clock time of each registered SimulationElement.
        /// </summary>
        internal record AdvanceTo
        {
            public Time Time { get; }
            public AdvanceTo(Time time)
            {
                Time = time;
            }
        }

        /// <summary>
        /// A Wrapper for messages to pop after given delay
        /// </summary>
        public record Schedule
        {
            /// <summary>
            /// Amount of TimeSteps the message should be delayed
            /// </summary>
            public Time AtTime { get; }
            public IHiveMessage Message { get; }
            public Schedule(Time atTime, IHiveMessage message)
            {
                AtTime = atTime;
                Message = message;
            }
        }

        /// <summary>
        /// General message envelope for all Messages in the System
        /// </summary>
        /// <param name="message"></param>
        /// <param name="target"></param>
        /// <param name="logThis">default: False</param>
        /// <param name="priority">default: Medium</param>
        public HiveMessage(object message, IActorRef target, bool logThis = false)
        {
            Key = Guid.NewGuid();
            Message = message;
            Target = target;
            Priority = Priority.Medium;
            LogThis = logThis;
        }

        /// <summary>
        /// Enables Message Tracing for this message by publishing it to event hub.
        /// </summary>
        /// <returns></returns>
        public IHiveMessage WithTraceing()
        {
            return this with { LogThis = true };
        }

        public IHiveMessage WithSender(IActorRef sender)
        {
            return this with { Sender = sender };
        }


        /// <summary>
        /// Altering the Message Priority
        /// VeryHigh = 100,
        /// High = 200,
        /// Medium = 300,
        /// Low  = 400,
        /// VeryLow = 500
        /// </summary>
        /// <param name="priority">default is Medium</param>
        /// <returns></returns>
        public IHiveMessage WithPriority(Priority priority)
        {
            return this with { Priority = priority };
        }


        /// <summary>
        /// Broadcast message by ActorSelection
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetSelection"></param>
        /// <param name="logThis"></param>
        /// <param name="priority"></param>
        protected HiveMessage(object message, ActorSelection targetSelection, bool logThis = false, Priority priority = Priority.Medium)
        {
            Key = Guid.NewGuid();
            Message = message;
            TargetSelection = targetSelection;
            Priority = priority;
            LogThis = logThis;
        }
    }

}
