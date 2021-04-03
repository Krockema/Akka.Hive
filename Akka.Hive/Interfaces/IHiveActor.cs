using System;
using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Interfaces
{
    public interface IHiveActor
    {
        Guid Key { get; }
        Time Time { get; }
        IActorRef Self { get; }
        IActorRef ContextManager { get; }
        ITimerScheduler Timers { get; }
    }
}
