using System;
using Akka.Hive.Interfaces;

namespace Akka.Hive.Definitions.Instructions
{
    public interface ICurrentInstructions
    {
        int Count();
        bool Remove(Guid msg);
        bool Add(Guid key, IHiveMessage message);
        void WaitForDiastole(bool token);
    }
}