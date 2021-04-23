using System;
using Akka.Hive.Definitions;

namespace Akka.Hive.Interfaces
{
    /// <summary>
    /// Basic actor functionality that every Hive resident has to implement.
    /// </summary>
    public interface IHiveAction
    {
        void Send(IHiveMessage instruction, TimeSpan waitFor = new());
        void Schedule(TimeSpan delay, IHiveMessage message);
        void ScheduleMessages(Time atTime, HiveMessage Message);
        void Finish(IHiveMessage finish);
        void PreStart();
        void PostStop();
    }
}