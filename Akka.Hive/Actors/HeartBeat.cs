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
        private TimeSpan _tickSpeed;
        /// <summary>
        /// Creation method for HeartBeat Actor
        /// </summary>
        /// <param name="tickSpeed">minimum time that is spend for each global tick</param>
        /// <returns></returns>
        public static Props Props(TimeSpan tickSpeed)
        {
            return Actor.Props.Create(() => new HeartBeat(tickSpeed));
        }
        
        public HeartBeat(TimeSpan timeToAdvance)
        {
            #region init
            _tickSpeed = timeToAdvance;
            #endregion
            
            Receive<HiveMessage.Command>(dl =>
                SendHeartBeat()
            );

            Receive<TimeSpan>(tta =>
                _tickSpeed = tta
            );
        }

        private void SendHeartBeat()
        {
            Task.Delay(_tickSpeed).Wait();
            Sender.Tell(HiveMessage.Command.HeartBeat);
        }

    }
}

