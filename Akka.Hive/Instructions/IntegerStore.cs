using System;
using Akka.Hive.Interfaces;
using Akka.Hive.Logging;
using NLog;

namespace Akka.Hive.Instructions
{
    /// <summary>
    /// Basic message store that only saves the amount of messages to process for the current virtual time
    /// </summary>
    public class IntegerStore : ICurrentInstructions
    {
        private int _store;
        private int _lastStoredMessageCount;
        private int _equalRounds;
        public NLog.Logger Logger;

        public IntegerStore()
        {
            Logger = LogManager.GetLogger(TargetNames.LOOP_DETECTION);
        }

        public bool Add(Guid key, IHiveMessage message)
        {
            _store++;
            return true;
        }

        public int Count()
        {
            return _store;
        }

        public void IntegrityCheck()
        {
            if (_lastStoredMessageCount == _store)
            {
                _equalRounds++;
                Logger.Log(LogLevel.Debug, $"Possible loop detected, msg count :{_lastStoredMessageCount} last changed {_equalRounds}");
            }
            _lastStoredMessageCount = _store;
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