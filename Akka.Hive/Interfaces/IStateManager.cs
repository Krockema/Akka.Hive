using Akka.Actor;

namespace Akka.Hive.Interfaces
{
    public interface IStateManagerBase { IWithHive WithHive(Hive hive); }
    public interface IWithHive { IWithInbox WithInbox(Inbox inbox); }
    public interface IWithInbox { StateManager Start(); }
}