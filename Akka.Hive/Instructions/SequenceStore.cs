using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Hive.Interfaces;
using Akka.Hive.Logging;
using NLog;

namespace Akka.Hive.Instructions
{
    /// <summary>
    /// ... that saves the message that shall be process within the current virtual time
    /// </summary>
    public class SequenceStore : ICurrentInstructions
    {
        private readonly SortedDictionary<Guid, IHiveMessage> _store = new();
        private int _wait = 0;
        private Guid _lastMessageKey;
        private int _lastStoredMessageCount;
        private int _equalRounds;
        private NLog.Logger _logger;

        public SequenceStore()
        {
            _logger = LogManager.GetLogger(TargetNames.LOOP_DETECTION);
            _lastMessageKey = Guid.Empty;
        }

        public bool Add(Guid key, IHiveMessage message)
        {
            return _store.TryAdd(key, message);
        }

        public int Count()
        {
            return _store.Count + _wait;
        }

        public void IntegrityCheck()
        {
            if (_store.Count == 0)
                return;

            if (_lastStoredMessageCount != _store.Count)
            {
                _equalRounds = 1;
                _lastStoredMessageCount = _store.Count;
                _lastMessageKey = _store.FirstOrDefault().Key;
                return;
            }
                
            // [ something might be wrong ]
            if (_store.ContainsKey(_lastMessageKey)) // something is wrong 
                _logger.Log(LogLevel.Debug, $"Possible deadlock detected, msg count :{_lastStoredMessageCount} " +
                                            $"last changed {_equalRounds}" +
                                            $"message {_store[_lastMessageKey].Message.GetType()}" +
                                            $"target {_store[_lastMessageKey].Target.Path.Address}" +
                                            $"sender {_store[_lastMessageKey].Message}");
            else // loop ?
                _logger.Log(LogLevel.Debug, $"Possible loop detected, msg count :{_lastStoredMessageCount} last changed {_equalRounds}");

            // count this was happening
            _equalRounds++;
        }

        public bool Remove(Guid msg)
        {
            return _store.Remove(msg);
        }

        public void WaitForDiastole(bool token)
        {
            _wait = token ? 1 : 0;
        }

        public IHiveMessage GetNext()
        {
            return _store.First().Value;
        }
    }
}