using System;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Hive.Action;
using Akka.Hive.Actors;
using Akka.Hive.Interfaces;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace Akka.Hive.Examples.Resources.Machine
{
    public class MachineMqttActions : Holon
    {
        private readonly string _name;
        private IManagedMqttClient _mqttClient;
        private string _topicIn;
        private string _topicOut;
        private readonly IActorRef _self;

        public MachineMqttActions(HiveActor actor) : base(actor)
        {
            _self = actor.Self;
            _name = actor.Self.Path.Name;
            if (actor is IWithExternalConnection match)
            {
                match.SendExtern = SendExtern;
            }
        }

        private void SendExtern(object obj)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(obj);

            Console.WriteLine($"Sending message with: {payload} | to topic: {_topicIn}.");
            _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(_topicIn)
                .WithPayload(Encoding.UTF8.GetBytes(payload)).Build()); 
        }

        public override void PreStart()
        {
            // not Required yet
            StartMqttClientTask().ConfigureAwait(true);
        }

        public override void PostStop()
        {
            // not Required yet
        }


        private Task StartMqttClientTask()
        {
            return Task.Run(async () =>
            {
                // Setup and start a managed MQTT client.
                var options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId("Client" + new Random().Next())
                        .WithTcpServer("127.0.0.1", 8800)
                        .Build())
                    .Build();

                _topicIn = $"ssop/{_name}_in";
                _topicOut = $"ssop/{_name}_out";
                
                _mqttClient = new MqttFactory().CreateManagedMqttClient();
                await _mqttClient.StartAsync(options);
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(_topicIn)
                    .Build());
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(_topicOut)
                    .Build());

                _mqttClient.UseApplicationMessageReceivedHandler(async e =>
                {
                    try
                    {
                        await Task.Run(() => {
                            string topic = e.ApplicationMessage.Topic;
                            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                            if (_topicOut == topic)
                            {   
                                Console.WriteLine($"Message received: {payload} | From Topic: {topic}.");
                                if (payload == _name)
                                    _self.Tell(new MachineAgent.MachineReady(null, _self));
                                else
                                    _self.Tell(new MachineAgent.FinishWork(null, _self));

                            } else if (_topicIn == topic)
                            {
                                Console.WriteLine($"Message Send and Returned: {payload} | From Topic: {topic}.");
                            } else { 
                                throw new Exception("bad request");
                            }

                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message, ex);
                    }
                });

                _mqttClient.UseConnectedHandler(e =>
                {
                    Console.WriteLine($"Connected successfully with MQTT Broker");
                    Console.WriteLine("Press enter to publish msg.");
                });
            });
        }
    }
}