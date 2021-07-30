using System;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Instructions
{
    /// <summary>
    /// Interface for the remaining messages to be processed in the current virtual time.
    /// </summary>
    public interface ICurrentInstructions
    {
        int Count();
        bool Remove(Guid msg);
        bool Add(Guid key, IHiveMessage message);
        void WaitForDiastole(bool token);
        void IntegrityCheck();
    }
}