using Akka.Actor;
using Akka.Hive.Action;
using System;

namespace Akka.Hive.Definitions
{
    public interface IHiveConfig
    {
        bool DebugAkka { get; }
        bool DebugHive { get; }
        TimeSpan InterruptInterval { get; }
        TimeSpan TickSpeed { get; }
        TimeSpan TimeSpanToTerminate { get; }
        Time StartTime { get; }
        ActionFactory ActorActionFactory { get; }
        MessageTrace MessageTrace { get; }
        Func<Hive, object[], Props> StateManagerProbs { get; set; }
        IActorRef StateManagerRef { get; set; }
    }

    public interface IHiveConfigBase
    {
        IHiveConfig Build();
        IHiveConfigBase WithDebugging(bool akka, bool hive);
        IHiveConfigBase WithInterruptInterval(TimeSpan timeSpan);
        IHiveConfigBase WithTickSpeed(TimeSpan timeSpan);
        IHiveConfigBase WithStartTime(Time timeSpan);
        IHiveConfigBase WithMessageTracer(MessageTrace tracer);
        IHiveConfigBase WithStateManagerProbs(Func<Hive, object[], Props> stateManagerProbs);
    }

    public interface IHiveConfigSimulation
    {
        IHiveConfigBase WithTimeSpanToTerminate(TimeSpan timeSpanToTerminate);

    }
    public interface IHiveConfigHolon
    {
        IHiveConfigBase WithActionFactory(ActionFactory actionFactory);
    }
}
