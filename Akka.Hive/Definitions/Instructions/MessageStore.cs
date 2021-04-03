using System;
using System.Collections.Generic;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Definitions.Instructions
{
    public class MessageStore : ICurrentInstructions
    {
        private readonly Dictionary<Guid, IHiveMessage> _store = new();
        private int _wait = 0;

        public bool Add(Guid key, IHiveMessage message)
        {
            return _store.TryAdd(key, message);
        }

        public int Count()
        {
            return _store.Count + _wait;
        }

        public bool Remove(Guid msg)
        {
            return _store.Remove(msg);
        }

        public void WaitForDiastole(bool token)
        {
            _wait = token ? 1 : 0;
        }
    }
}