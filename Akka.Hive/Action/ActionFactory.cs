using System;
using Akka.Hive.Actors;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Action
{
    /// <summary>
    /// provides Actor functionality based on the given implementation. Can be overwritten to create new functions. I.e. MQTT connections.
    /// </summary>
    public class ActionFactory
    {
        /// <summary>
        /// References the current target type
        /// Default => Simulation
        /// Custom => Holon
        /// </summary>
        public ActionsType ActorActions { get; }
        private Func<HiveActor, IHiveAction> ActionCreator { get; }

        /// <summary>
        /// Creates default actor ActionFactory with simulation environment
        /// </summary>
        public ActionFactory()
        {
            ActorActions = ActionsType.Simulation;
            ActionCreator = (actor) => new Simulation(actor);
        }

        /// <summary>
        /// Creates customized actor ActionFactory that targets an IHiveAction implementation.
        /// </summary>
        /// <param name="actorCreator">Custom actor creation function</param>
        public ActionFactory(Func<HiveActor, IHiveAction> actorCreator)
        {
            ActorActions = ActionsType.Holon;
            ActionCreator = actorCreator;
        }


        public virtual IHiveAction Create(HiveActor actor)
        {
            return ActionCreator(actor);
        }
    }
}