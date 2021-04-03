using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hive.Definitions;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// Actor that regulates the Simulation Speed.
    /// </summary>
    public class HeartBeat :  ReceiveActor
    {
        private TimeSpan _timeToAdvance;
        /// <summary>
        /// Creation method for HeartBeat Actor
        /// </summary>
        /// <param name="timeToAdvance">minimum time that is spend for each global tick</param>
        /// <returns></returns>
        public static Props Props(TimeSpan timeToAdvance)
        {
            return Actor.Props.Create(() => new HeartBeat(timeToAdvance));
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

