using System;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Instructions
{
    /// <summary>
    /// Basic message store that only saves the amount of messages to process for the current virtual time
    /// </summary>
    public class IntegerStore : ICurrentInstructions
    {
        private int _store;

        public bool Add(Guid key, IHiveMessage message)
        {
            _store++;
            return true;
        }

        public int Count()
        {
            return _store;
        }

        public bool Remove(Guid msg)
        {
            _store--;
            return true;
        }

        public void WaitForDiastole(bool token)
        {
            _store += token ? +1 : -1;
        }
    }
}