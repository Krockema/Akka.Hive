using System;
using Akka.Hive.Actors;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Action
{
    public class ActionFactory
    {
        /// <summary>
        /// References the current target type
        /// Default => Simulation
        /// Custom => Holon
        /// </summary>
        public Actions ActorActions { get; }
        private Func<HiveActor, IHiveAction> ActionCreator { get; }

        /// <summary>
        /// Creates default actor ActionFactory with simulation environment
        /// </summary>
        public ActionFactory()
        {
            ActorActions = Actions.Simulation;
            ActionCreator = (actor) => new Simulation(actor);
        }

        /// <summary>
        /// Creates customized actor ActionFactory that targets an IHiveAction implementation.
        /// </summary>
        /// <param name="actorCreator">Custom actor creation function</param>
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