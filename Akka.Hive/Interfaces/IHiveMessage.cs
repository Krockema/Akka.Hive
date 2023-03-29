using System;
using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Interfaces
{
    /// <summary>
    /// Interface that any message that is passed between actors have to implement.
    /// </summary>
    public interface IHiveMessage
    {
        Guid Key { get; }
        object Message { get; }
        IActorRef Target { get; }
        IActorRef Sender { get; }
        Priority Priority { get; }
        IHiveMessage WithSender(IActorRef sender);
        bool LogThis { get;  }
        int CompareTo(IHiveMessage other);
    }
}