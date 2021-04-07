using System;
using System.Text;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace Akka.Hive.Examples.MqttServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(8800)
                .WithConnectionValidator(c => {
                    LogThis(c);
                    c.ReasonCode = MqttConnectReasonCode.Success;
                }).WithSubscriptionInterceptor(c => {
                    LogThis(c);
                    c.AcceptSubscription = true;
                }).WithApplicationMessageInterceptor(c =>
                {
                    LogThis(c);
                    c.AcceptPublish = true;
                });
            ;
            var mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(optionsBuilder.Build());

            Console.WriteLine("Server is up and running");
            
            while (true)
            {
                var input = Console.ReadLine();
                if (input != "")
                {
                    mqttServer.PublishAsync(new MqttApplicationMessageBuilder()
                        .WithTopic("ssop/in")
                        .WithPayload(Encoding.UTF8.GetBytes(input)).Build());    
                }
            }
        }
        private static void LogThis(MqttConnectionValidatorContext intercept)
        {
            string clientId = intercept.ClientId;

            if (string.IsNullOrWhiteSpace(clientId) == false)
            {
                string endpoint = intercept.Endpoint;
                Console.WriteLine($"Connection try from: {clientId}. From: {endpoint}");
            }
        }

        private static void LogThis(MqttSubscriptionInterceptorContext intercept)
        {
            string topic = intercept.TopicFilter.Topic;

            if (string.IsNullOrWhiteSpace(topic) == false)
            {
                string endpoint = intercept.ClientId;
                Console.WriteLine($"Subscription to {topic} from: {endpoint} received");
            }
        }
        private static void LogThis(MqttApplicationMessageInterceptorContext intercept)
        {
            string topic = intercept.ApplicationMessage.Topic;

            if (string.IsNullOrWhiteSpace(topic) == false)
            {
                string payload = Encoding.UTF8.GetString(intercept.ApplicationMessage.Payload);
                Console.WriteLine($"Forwarding to Topic: {topic}. Message: {payload}");
            }
        }
    }
}
