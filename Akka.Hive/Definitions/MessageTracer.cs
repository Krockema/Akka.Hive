using Akka.Event;
using Akka.Hive.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.Hive.Definitions
{
    public class MessageTrace 
    {
        private Dictionary<Type, HashSet<Type>> Tracing = new ();
        public MessageTrace() { }

        public MessageTrace AddTrace(Type agentType, Type messageType)
        {
            if(Tracing.TryGetValue(agentType,  out HashSet<Type> messages))
                    messages.Add(messageType);
            else Tracing.Add(agentType, new HashSet<Type>() { messageType });
            return this;
        }

        public Action<IHiveMessage, EventStream> GetTracer(IHiveActor agentType)
        {
            if (Tracing.TryGetValue(agentType.GetType(), out HashSet<Type> messages))
                return (msg, stream) => 
                { 
                    if(messages.Contains(msg.GetType()))
                        stream.Publish(msg); 
                };
            return (action, stream) => { return; };
        }

        public List<Type> GetTracedMessages(Type agentType)
        {
            if (Tracing.TryGetValue(agentType, out HashSet<Type> messages))
                return messages.ToList();
            throw new Exception("Type is not configured");
        }
    }
}
