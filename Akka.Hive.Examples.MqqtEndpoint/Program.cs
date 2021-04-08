using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AkkaMQTT
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Resource MQTT Endpoint, what client do you want to connect? Machine 1,2 or 3?");
            var connectTo = Console.ReadLine();
            var topic_in = $"ssop/Machine_{connectTo}_in";
            var topic_out = $"ssop/Machine_{connectTo}_out";
            var name = $"Machine_{connectTo}";
            
            Console.WriteLine($"Trying to connect to {topic_in} and {topic_out}");

            // Setup and start a managed MQTT client.
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("Client" + new Random().Next())
                    .WithTcpServer("127.0.0.1", 8800)
                    .Build())
                .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            await mqttClient.StartAsync(options);
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                            .WithTopic(topic_in)
                            .Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                            .WithTopic(topic_out)
                            .Build());
            
            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                try
                {
                    await Task.Run(() => {
                        string topic = e.ApplicationMessage.Topic;
                        if (topic == topic_in)
                        {
                            string[] payload = System.Text.Json.JsonSerializer.Deserialize<string[]>(e.ApplicationMessage.Payload);
                            DoWorkTask(topic, payload, mqttClient, topic_out);
                        } else if (topic == topic_out)
                        {
                            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
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

            mqttClient.UseConnectedHandler(e =>
            {
                Console.WriteLine("Connected successfully with MQTT Broker");
                Console.WriteLine("Enter 'ready' to signal ready for work");
            });


            while (true)
            {
                var input = Console.ReadLine();
                if (input == "ready")
                {
                    await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                        .WithTopic(topic_out)
                        .WithPayload(Encoding.UTF8.GetBytes(name)).Build());    
                }
            }
        }

        private static Task DoWorkTask(string fromTopic, string[] payload, IManagedMqttClient client, string targetTopic)
        {
            Console.WriteLine($"Message received: {payload[0]} | From Topic: {fromTopic}.");
            Working(int.Parse(payload[1]));
            return client.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(targetTopic)
                .WithPayload(Encoding.UTF8.GetBytes("Done")).Build()); 
        }

        private static void Working(int duration)
        {
            var barInline = "";
            for (int j = 0; j < duration; j++)
            {
                barInline += " ";
            }
            Console.Write("[" + barInline + "]");

            for (int i = 0; i < duration; i++)
            {
                Console.SetCursorPosition(i+1, Console.CursorTop);
                Console.Write("/");
                Task.Delay(TimeSpan.FromSeconds(0.333)).Wait();
                
                Console.SetCursorPosition(i+1, Console.CursorTop);
                Console.Write("-");
                Task.Delay(TimeSpan.FromSeconds(0.333)).Wait();
                
                Console.SetCursorPosition(i+1, Console.CursorTop);
                Console.Write("\\");
                Task.Delay(TimeSpan.FromSeconds(0.334)).Wait();

                Console.SetCursorPosition(i+1, Console.CursorTop);
                Console.Write("-");
            }
            Console.WriteLine("\rDone. . .  Sending finish work.");
        }
    }
}
