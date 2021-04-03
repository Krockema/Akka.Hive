using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// A Time Monitor that regulates the Simulation Speed.
    /// </summary>
    public class HeartBeat :  ReceiveActor
    {
        private TimeSpan _timeToAdvance;
        public static Props Props(TimeSpan timeToAdvance)
        {
            return Akka.Actor.Props.Create(() => new HeartBeat(timeToAdvance));
        }
        public HeartBeat(TimeSpan timeToAdvance)
        {
            #region init
            _timeToAdvance = timeToAdvance;
            #endregion
            
            Receive<HiveMessage.Command>(dl =>
                SendHeartBeat()
            );

            Receive<TimeSpan>(tta =>
                _timeToAdvance = tta
            );
        }

        private void SendHeartBeat()
        {
            Task.Delay(_timeToAdvance).Wait();
            Sender.Tell(HiveMessage.Command.HeartBeat);
        }

    }
}

