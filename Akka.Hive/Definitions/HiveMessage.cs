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
        /// </summary>
        public object Message { get; }
        /// <summary>
        /// Target Actor to whom the Simulation Message shall be forwarded.
        /// </summary>
        public IActorRef Target { get; }
        /// <summary>
        /// Priority to Order msg
        /// </summary>
        public Priority Priority { get; }
        /// <summary>
        /// Log this Message to the event Stream.
        /// </summary>
        public bool LogThis { get; }        
        /// <summary>
        /// For simple and fast internal instructions
        /// </summary>
        public enum Command
        {
            Start,
            Stop,
            Done,
            Finish,
            IsReady,
            HeartBeat
        }

        /// <summary>
        /// Used to build a start / stop / mechanic with this 
        /// </summary>
        public enum SimulationState
        {
            Stopped,
            Started,
            Finished,
            Bounced
        }

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
            public Shutdown(IActorRef simulationContextRef) : base(null, simulationContextRef, false, Priority.Low) { }
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
            internal Schedule(Time atTime, IHiveMessage message)
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
        public HiveMessage(object message, IActorRef target, bool logThis = false, Priority priority = Priority.Medium)
        {
            Key = Guid.NewGuid();
            Message = message;
            Target = target;
            Priority = priority;
            LogThis = logThis;
        }

        /// <summary>
        /// Broadcast message by ActorSelection
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetSelection"></param>
        /// <param name="logThis"></param>
        /// <param name="priority"></param>
        protected HiveMessage(object message, ActorSelection targetSelection,bool logThis = false, Priority priority = Priority.Medium)
        {
            Key = Guid.NewGuid();
            Message = message;
            TargetSelection = targetSelection;
            Priority = priority;
            LogThis = logThis;
        }
    }

}
