using Akka.Actor;

namespace Akka.Hive.Test.Moc
{
    public class PingBackAndForward : ReceiveActor
    {
        public const string Pong = "PONG";
        IActorRef target = ActorRefs.NoSender;

        public PingBackAndForward()
        {
            Receive<string>(s => s.Equals(Ping.Name), (message) => {
                Sender.Tell(Pong, Self);
                if (!target.Equals(ActorRefs.NoSender))
                    target.Forward(message);
            });

            Receive<IActorRef>(actorRef => {
                target = actorRef;
                Sender.Tell("done");
            });
        }

    }
}
