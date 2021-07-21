using System;
using Akka.Actor;
using Akka.Event;
using Akka.Hive.Action;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;
using Akka.Hive.Logging;
using NLog;
using static Akka.Hive.Definitions.HiveMessage;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// Abstract implementation of the Hive resident, that is to be implemented for the needs.
    /// </summary>
    public abstract class HiveActor : ReceiveActor, IHiveActor, ILogReceive, IWithTimers
    {
        /// <summary>
        /// Referencing the Global Actor Context. (Head Actor)
        /// </summary>
        public IActorRef ContextManager { get; }

        /// <summary>
        /// Not used for Simulation Approach! This is the Akka internal timer for future task Scheduling in RealTime
        /// </summary>
        public ITimerScheduler Timers { get; set; }

        public NLog.Logger Logger { get; }
        /// <summary>
        /// Guid of the current element
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// Current Time
        /// </summary>
        public Time Time { get; internal set; }

        public new IActorRef Self => base.Self;

        internal IUntypedActorContext ActorContext => Context;

        /// <summary>
        /// Specifies the internal handling of messages.
        /// Possible Types:
        /// - Holon
        /// - Simulation
        /// </summary>
        private IHiveAction ActorActions { get; }

        private Action<IHiveMessage, EventStream> Trace { get; }

        /// <summary>
        /// Register the simulation element at the Simulation
        /// </summary>
        protected sealed override void PreStart()
        {
            ActorActions.PreStart();
            base.PreStart();
        }

        public IHiveConfig EngineConfig {get;} 

        protected HiveActor(IActorRef simulationContext, Time time, IHiveConfig hiveConfig)
        {
            #region Init

            ActorActions = hiveConfig.ActorActionFactory.Create(this);
            Key = Guid.NewGuid();
            Logger = LogManager.GetLogger(TargetNames.LOG_ACTORS);
            EngineConfig = hiveConfig;
            Time = time;
            ContextManager = simulationContext;
            Trace = hiveConfig.MessageTrace.GetTracer(this);
            
            #endregion Init

            switch (hiveConfig.ActorActionFactory.ActorActions)
            {
                case ActionsType.Holon : Become(Holon); break;
                case ActionsType.Simulation : Become(Simulant); break;
                default : throw new Exception($"Actor type specification unknown, actor type : {hiveConfig.ActorActionFactory.ActorActions}");
            };
        }

        private void Holon()
        {
            Receive<Schedule>(message => Send(message.Message));
            Receive<Finish>(f => ActorActions.Finish(f));
            ReceiveAny(a =>
            {
                Time = Time.Now;
                Do(a);
            });
        }

        private void Simulant()
        {
            var act = ActorActions as Simulation;
            Receive<Finish>(f => act.Finish(f));
            Receive<Schedule>(message => act.ScheduleMessages(message.AtTime, (HiveMessage)message.Message));
            Receive<AdvanceTo>(m => act.AdvanceTo(m.Time));
            Receive<IDirectMessage>(message => Do(message));
            ReceiveAny(act.MapMessageToMethod);
        }

        protected internal void Send(IHiveMessage instruction, TimeSpan waitFor = new ())
        {
            ActorActions.Send(instruction, waitFor);
            Trace(instruction, ActorContext.System.EventStream);
        }

        protected internal void Schedule(TimeSpan waitFor, IHiveMessage instruction)
        {
            ActorActions.Schedule(waitFor, instruction);
            Trace(instruction, ActorContext.System.EventStream);
        }
        
        /// <summary>
        /// Deregister the Actor from Context and Tell parrent Elements that his work is done.
        /// </summary>
        protected sealed override void PostStop()
        {
            ActorActions.PostStop();
            base.PostStop();
        }
        /// <summary>
        /// check if all childs Finished
        /// if there is any path which is not equal to the child path not all childs have been terminated.
        /// Question to Check: ?? Should be GetChildren = null ?? to ensure there are no childs anymore... ?? // MK
        /// </summary>
        private void Terminate()
        {
            var childs = Context.GetChildren();
            foreach (var child in childs)
                if (child.Path != Sender.Path) return;

            // geratefully shutdown
            Context.Stop(Self);
        }
        /// <summary>
        /// Free for implementing
        /// is called before the simulation clock is advancing one tick 
        /// and this is only called by simulation system.
        /// </summary>
        protected internal virtual void PostAdvance()
        {

        }


        /// <summary>
        /// Free for implementing your own behave on messages 
        /// </summary>
        /// <param name="process"></param>
        protected internal abstract void Do(object process);


        /// <summary>
        /// Anything that has to be done before shutdown, default it will terminate the Actor if the Actor hase no more childs.
        /// </summary>
        /// <param name="process"></param>
        protected internal virtual void Finish()
        {
            Terminate();
        }
    }
}