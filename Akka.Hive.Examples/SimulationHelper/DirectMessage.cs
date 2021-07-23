using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Hive.Examples.SimulationHelper
{
    public record RequestStatistics : HiveMessage, IDirectMessage
    {
        public RequestStatistics(IActorRef target): base(target: target, message: null)
        {

        }
    }
}
