using System;

namespace Akka.Hive.Instructions
{
    /// <summary>
    /// Interface for the messages that are promised to be processed in the future.
    /// </summary>
    public interface IFeatureInstructions
    {
        int Count();
        bool TryGetValue(DateTime time, out ICurrentInstructions feature);
        void Add(DateTime scheduleAt, ICurrentInstructions instructions);
        DateTime Next();
        void Remove(DateTime timePeriod);
    }
}