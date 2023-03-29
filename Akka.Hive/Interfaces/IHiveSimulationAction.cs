
using Akka.Hive.Definitions;

namespace Akka.Hive.Interfaces
{
    public interface IHiveSimulationAction
    {
        void MapMessageToMethod(IHiveMessage message);
        void AdvanceTo(Time message);

    }
}
