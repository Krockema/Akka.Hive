using System;
using Akka.Actor;
using Akka.Hive.Action;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;
using Akka.Hive.Logging;
using NLog;
using static Akka.Hive.Definitions.HiveMessage;

namespace Akka.Hive.Actors
{
    public abstract class HiveActor : ReceiveActor, IHiveActor, ILogReceive, IWithTimers
    {
        /// <summary>
        /// Referencing the Global Actor Context. (Head Actor)
        /// </summary>
        public IActorRef ContextManager { get; }

        /// <summary>
        /// Not used for Simulation Approach! THis is the Akka Internal Timer for future task Scheduling in RealTime
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

        /// <summary>
        /// Register the simulation element at the Simulation
        /// </summary>
        protected sealed override void PreStart()
        {
            ActorActions.PreStart();
            base.PreStart();
        }

        public HiveConfig EngineConfig {get;} 

        protected HiveActor(IActorRef simulationContext, Time time, HiveConfig engineConfig)
        {
            #region Init

            ActorActions = engineConfig.ActorActionFactory.Create(this);
            Key = Guid.NewGuid();
            Logger = LogManager.GetLogger(TargetNames.LOG_AGENTS);
            EngineConfig = engineConfig;
            Time = time;
            ContextManager = simulationContext;
            
            #endregion Init

            switch (engineConfig.ActorActionFactory.ActorActions)
            {
                case Actions.Holon : Become(Holon); break;
                case Actions.Simulation : Become(Simulant); break;
                default : throw new Exception($"Actor type specification unknown, actor type : {engineConfig.ActorActionFactory.ActorActions}");
            };
        }

        private void Holon()
        {
            Receive<Schedule>(message => ActorActions.ScheduleMessages(message.AtTime, (HiveMessage)message.Message));
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
            ReceiveAny(act.MapMessageToMethod);
        }


        protected internal void Send(IHiveMessage instruction, TimeSpan waitFor = new ())
        {
            ActorActions.Send(instruction, waitFor);
        }

        protected internal void Schedule(TimeSpan waitFor, IHiveMessage instruction)
        {
            ActorActions.Schedule(waitFor, instruction);
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

        protected internal virtual void PostAdvance()
        {

        }


        /// <summary>
        /// Free for implementing your own behave on messages 
        /// </summary>
        /// <param name="process"></param>
        protected internal virtual void Do(object process)
        {
            
        }

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