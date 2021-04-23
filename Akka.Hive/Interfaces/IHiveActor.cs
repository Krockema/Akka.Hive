using System;
using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Interfaces
{
    /// <summary>
    /// Basic interface that any Hive resident has to implement.
    /// </summary>
    public interface IHiveActor
    {
        Guid Key { get; }
        Time Time { get; }
        IActorRef Self { get; }
        IActorRef ContextManager { get; }
        ITimerScheduler Timers { get; }
    }
}
