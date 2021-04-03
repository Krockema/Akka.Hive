using System;
using System.Collections.Generic;
using System.Linq;

namespace Akka.Hive.Definitions.Instructions
{
    public class FeatureStore : IFeatureInstructions
    {

        private readonly SortedDictionary<DateTime, ICurrentInstructions> _store = new ();
        public int Count()
        {
            return _store.Count;
        }

        public bool TryGetValue(DateTime time, out ICurrentInstructions feature)
        {
            return _store.TryGetValue(time, out feature);

        }

        public void Add(DateTime scheduleAt, ICurrentInstructions instructions)
        {
            _store.Add(scheduleAt, instructions);
        }

        public DateTime Next()
        {
            return _store.Keys.First();
        }

        public void Remove(DateTime timePeriod)
        {
            _store.Remove(timePeriod);
        }
    }
}