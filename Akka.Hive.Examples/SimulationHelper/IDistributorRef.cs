using Akka.Actor;
using Akka.Hive.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Hive.Examples.SimulationHelper
{
    public interface IWithDistributorRef 
    {
        IStateManagerBase WithDistributor(IActorRef actorRef);
    }
}
