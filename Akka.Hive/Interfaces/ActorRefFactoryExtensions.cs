using System;
using System.Linq.Expressions;
using Akka.Actor;

namespace Akka.Hive.Interfaces
{
    public static class ActorRefFactoryExtensions
    {
        /// <summary>
        /// Creates an actor, creating props based on the provided
        /// actor factory method.
        /// </summary>
        /// <typeparam name="T">The actor type.</typeparam>
        /// <param name="actorRefFactory">ActorSystem or actor Context.</param>
        /// <param name="actorFactory">Actor factory method.</param>
        public static IActorRef CreateActor<T>(this IActorRefFactory actorRefFactory,
            Expression<Func<T>> actorFactory) where T : ActorBase
        {
            var props = Props.Create(actorFactory);
            var actor = actorRefFactory.ActorOf(props);
            return actor;
        }
    }


}
