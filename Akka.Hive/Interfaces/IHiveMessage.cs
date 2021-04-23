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
        Priority Priority { get; }
        bool LogThis { get;  }
        int CompareTo(IHiveMessage other);
    }
}