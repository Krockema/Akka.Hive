using System;
using Akka.Hive.Actors;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Action
{
    public class ActionFactory
    {
        public Actions ActorActions { get; }
        private Func<HiveActor, IHiveAction> ActionCreator { get; }

        /// <summary>
        /// Creates Default ActorAction Factory with simulation Environment
        /// </summary>
        public ActionFactory()
        {
            ActorActions = Actions.Simulation;
            ActionCreator = (actor) => new Simulation(actor);
        }

        /// <summary>
        /// Creates Customized ActorAction Factory that targets Holon (IActorAction).
        /// </summary>
        public ActionFactory(Func<HiveActor, IHiveAction> actorCreator)
        {
            ActorActions = Actions.Holon;
            ActionCreator = actorCreator;
        }


        public virtual IHiveAction Create(HiveActor actor)
        {
            return ActionCreator(actor);
        }
    }
}