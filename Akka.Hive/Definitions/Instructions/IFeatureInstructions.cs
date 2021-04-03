using System;

namespace Akka.Hive.Definitions.Instructions
{
    public interface IFeatureInstructions
    {
        int Count();
        bool TryGetValue(DateTime time, out ICurrentInstructions feature);
        void Add(DateTime scheduleAt, ICurrentInstructions instructions);
        DateTime Next();
        void Remove(DateTime timePeriod);
    }
}