using Akka.Actor;
using Akka.Hive.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Inbox Inbox { get; set; }
    }

    public interface IHiveConfigBase
    {
        IHiveConfig Build();
        IHiveConfigBase WithDebugging(bool akka, bool hive);
        IHiveConfigBase WithInterruptInterval(TimeSpan timeSpan);
        IHiveConfigBase WithTickSpeed(TimeSpan timeSpan);
        IHiveConfigBase WithStartTime(Time timeSpan);
        IHiveConfigBase WithMessageTracer(MessageTrace tracer);
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
