using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Logging;
using NLog;

namespace Akka.Hive.Actors
{
    /// <summary>
    /// Actor that regulates the Simulation Speed.
    /// </summary>
    public class HeartBeat :  ReceiveActor
    {
        private readonly NLog.Logger _logger = LogManager.GetLogger(TargetNames.LOG_AKKA);
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

            Receive<SimulationCommand>(dl =>
                SendHeartBeat()
            );

            Receive<TimeSpan>(tta =>
                _tickSpeed = tta
            );
        }

        private void SendHeartBeat()
        {
            _logger.Log(LogLevel.Debug, "Beat");
            Task.Delay(_tickSpeed).Wait();
            Sender.Tell(SimulationCommand.HeartBeat);
        }

    }
}

